using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Utils;

namespace CCLBStudio.StringIds
{
    /// <summary>
    /// Editor helpers to draw any <see cref="StringId"/> property as a popup, with optional "New Id" / "Delete" buttons
    /// that rewrite the static readonly fields of the id's source file.
    /// </summary>
    public static class StringIdEditorGUI
    {
        private class IdCache
        {
            public Type IdType;
            public List<StringId> Ids;
            public GUIContent[] Options;
            public HashSet<string> TakenNames;
        }

        private class FieldState
        {
            public bool Creating;
            public bool FocusNextFrame;
            public string NewId = string.Empty;
        }

        private static readonly Dictionary<Type, IdCache> Caches = new();
        private static readonly Dictionary<string, FieldState> States = new();
        private static readonly Regex IdentifierRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$");
        private const string NewIdFocusPrefix = "StringIdNewIdField_";
        private const float ButtonSpacing = 2f;
        private const float DeleteButtonWidth = 55f;
        private const float NewButtonWidth = 42f;
        private const float AddButtonWidth = 42f;
        private const float CancelButtonWidth = 55f;
        private static float HelpBoxHeight => EditorGUIUtility.singleLineHeight * 1.5f;

        #region Public API

        /// <summary>
        /// Draws a StringId property with GUILayout: popup + Delete / New Id buttons. The id type is inferred from the property.
        /// </summary>
        public static void DrawIdLayout(SerializedProperty property, GUIContent label = null, bool required = true, bool allowEdit = true)
        {
            Type idType = ResolveIdType(property);
            if (idType == null)
            {
                EditorGUILayout.HelpBox($"Unable to resolve StringId type for property '{property.propertyPath}'.", MessageType.Error);
                return;
            }

            DrawIdLayout(property, idType, label, required, allowEdit);
        }

        public static void DrawIdLayout<T>(SerializedProperty property, GUIContent label = null, bool required = true, bool allowEdit = true) where T : StringId
        {
            DrawIdLayout(property, typeof(T), label, required, allowEdit);
        }

        public static void DrawIdLayout(SerializedProperty property, Type idType, GUIContent label, bool required, bool allowEdit)
        {
            Rect position = EditorGUILayout.GetControlRect(true, GetIdHeight(property, idType, required, allowEdit));
            DrawId(position, property, label ?? new GUIContent(property.displayName), idType, required, allowEdit);
        }

        /// <summary>
        /// Height needed by <see cref="DrawId"/> for the current state of the property.
        /// </summary>
        public static float GetIdHeight(SerializedProperty property, Type idType, bool required = true, bool allowEdit = true)
        {
            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            var idProperty = property.FindPropertyRelative(StringId.IdProperty);
            if (idProperty == null) return line;

            float height = line;
            if (required && string.IsNullOrEmpty(idProperty.stringValue) && !idProperty.hasMultipleDifferentValues)
            {
                height += HelpBoxHeight + spacing;
            }

            if (allowEdit && TryGetState(property, out var state) && state.Creating)
            {
                height += line + spacing;
                if (GetNewIdError(state.NewId, idType) != null)
                {
                    height += HelpBoxHeight + spacing;
                }
            }

            return height;
        }

        /// <summary>
        /// Draws a StringId property in a rect: popup + Delete / New buttons (usable in PropertyDrawers, height given by <see cref="GetIdHeight"/>).
        /// </summary>
        public static void DrawId(Rect position, SerializedProperty property, GUIContent label, Type idType, bool required = true, bool allowEdit = true)
        {
            var idProperty = property.FindPropertyRelative(StringId.IdProperty);
            if (idProperty == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent($"'{property.propertyPath}' is not a StringId"));
                return;
            }

            float line = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            label ??= GUIContent.none;
            string current = idProperty.stringValue;
            bool isEmpty = string.IsNullOrEmpty(current);
            bool mixed = idProperty.hasMultipleDifferentValues;
            var row = new Rect(position.x, position.y, position.width, line);

            if (required && isEmpty && !mixed)
            {
                EditorGUI.HelpBox(new Rect(row.x, row.y, row.width, HelpBoxHeight), "A valid ID is required", MessageType.Error);
                row.y += HelpBoxHeight + spacing;
            }

            if (!allowEdit)
            {
                DrawIdPopup(row, property, label, idType);
                return;
            }

            var state = GetState(property);
            bool canDelete = !isEmpty && !mixed;
            float buttonsWidth = (canDelete ? DeleteButtonWidth + ButtonSpacing : 0f) + (state.Creating ? 0f : NewButtonWidth + ButtonSpacing);

            var popupRect = new Rect(row.x, row.y, row.width - buttonsWidth, line);
            DrawIdPopup(popupRect, property, label, idType);

            // Buttons are drawn with indentLevel 0 so they are not shifted inside their rects.
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;

            float x = popupRect.xMax + ButtonSpacing;
            if (canDelete)
            {
                if (GUI.Button(new Rect(x, row.y, DeleteButtonWidth, line), "Delete")
                    && EditorUtility.DisplayDialog("Delete id", $"Delete '{current}' from {idType.Name}?\nAny code referencing {idType.Name}.{current} will no longer compile.", "Delete", "Cancel")
                    && DeleteId(idType, current))
                {
                    idProperty.stringValue = string.Empty;
                }
                x += DeleteButtonWidth + ButtonSpacing;
            }

            if (!state.Creating && GUI.Button(new Rect(x, row.y, NewButtonWidth, line), "New"))
            {
                state.Creating = true;
                state.FocusNextFrame = true;
            }

            if (state.Creating)
            {
                row.y += line + spacing;
                if (label != null && label != GUIContent.none && !string.IsNullOrEmpty(label.text))
                {
                    row.xMin = popupRect.x + EditorGUIUtility.labelWidth;
                }
                DrawNewIdSection(row, property, idProperty, idType, state);
            }

            EditorGUI.indentLevel = indent;
        }

        /// <summary>
        /// Draws a StringId property as a single-line popup (usable in PropertyDrawers).
        /// </summary>
        public static void DrawIdPopup(Rect position, SerializedProperty property, GUIContent label, Type idType)
        {
            var idProperty = property.FindPropertyRelative(StringId.IdProperty);
            var cache = GetCache(idType);
            string current = idProperty.stringValue;

            int index = string.IsNullOrEmpty(current) ? 0 : cache.Ids.FindIndex(x => x.Id == current) + 1;
            bool missing = !string.IsNullOrEmpty(current) && index == 0;
            var options = cache.Options;
            if (missing)
            {
                options = options.Append(new GUIContent($"{current} (missing)")).ToArray();
                index = options.Length - 1;
            }

            label = EditorGUI.BeginProperty(position, label, property);
            EditorGUI.showMixedValue = idProperty.hasMultipleDifferentValues;
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label, index, options);
            if (EditorGUI.EndChangeCheck() && !(missing && newIndex == index))
            {
                idProperty.stringValue = newIndex == 0 ? string.Empty : cache.Ids[newIndex - 1].Id;
            }
            EditorGUI.showMixedValue = false;
            EditorGUI.EndProperty();
        }

        public static IReadOnlyList<StringId> GetIds(Type idType) => GetCache(idType).Ids;

        /// <summary>
        /// Finds the concrete StringId type of a property (works for fields, arrays and lists).
        /// </summary>
        public static Type ResolveIdType(SerializedProperty property)
        {
            string typeName = property.type;
            return TypeCache.GetTypesDerivedFrom<StringId>().FirstOrDefault(t => !t.IsAbstract && t.Name == typeName);
        }

        #endregion

        #region New Id

        private static void DrawNewIdSection(Rect row, SerializedProperty property, SerializedProperty idProperty, Type idType, FieldState state)
        {
            float line = EditorGUIUtility.singleLineHeight;
            string focusName = NewIdFocusPrefix + property.propertyPath;
            float textWidth = row.width - AddButtonWidth - CancelButtonWidth - 2 * ButtonSpacing;

            GUI.SetNextControlName(focusName);
            state.NewId = EditorGUI.TextField(new Rect(row.x, row.y, textWidth, line), state.NewId).Trim().Capitalize();
            if (state.FocusNextFrame && Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl(focusName);
                state.FocusNextFrame = false;
            }

            string error = GetNewIdError(state.NewId, idType);
            float x = row.x + textWidth + ButtonSpacing;

            using (new EditorGUI.DisabledScope(state.NewId.Length == 0 || error != null))
            {
                if (GUI.Button(new Rect(x, row.y, AddButtonWidth, line), "Add") && AddId(idType, state.NewId))
                {
                    idProperty.stringValue = state.NewId;
                    ResetState(state);
                    GUI.FocusControl(null);
                }
            }

            if (GUI.Button(new Rect(x + AddButtonWidth + ButtonSpacing, row.y, CancelButtonWidth, line), "Cancel"))
            {
                ResetState(state);
                GUI.FocusControl(null);
            }

            if (error != null && state.Creating)
            {
                EditorGUI.HelpBox(new Rect(row.x, row.y + line + EditorGUIUtility.standardVerticalSpacing, row.width, HelpBoxHeight), error, MessageType.Error);
            }
        }

        private static string GetNewIdError(string newId, Type idType)
        {
            if (string.IsNullOrEmpty(newId)) return null;
            if (GetCache(idType).TakenNames.Contains(newId)) return "Id already exists";
            if (!IdentifierRegex.IsMatch(newId) || newId == idType.Name) return "Id must be a valid C# identifier";
            return null;
        }

        private static void ResetState(FieldState state)
        {
            state.Creating = false;
            state.FocusNextFrame = false;
            state.NewId = string.Empty;
        }

        #endregion

        #region Source File Edition

        /// <summary>
        /// Adds a <c>public static readonly T NewId = new("NewId");</c> line to the id's source file.
        /// </summary>
        public static bool AddId(Type idType, string newId)
        {
            string path = FindScriptPath(idType);
            if (path == null) return false;

            var lines = File.ReadAllLines(path).ToList();
            var fieldRegex = new Regex($@"^(\s*)public\s+static\s+readonly\s+{Regex.Escape(idType.Name)}\s+\w+\s*=");

            int lastFieldIndex = lines.FindLastIndex(l => fieldRegex.IsMatch(l));
            int insertIndex;
            string indent;
            if (lastFieldIndex >= 0)
            {
                insertIndex = lastFieldIndex + 1;
                indent = fieldRegex.Match(lines[lastFieldIndex]).Groups[1].Value;
            }
            else
            {
                var classRegex = new Regex($@"^(\s*).*\bclass\s+{Regex.Escape(idType.Name)}\b");
                int classIndex = lines.FindIndex(l => classRegex.IsMatch(l));
                int braceIndex = classIndex < 0 ? -1 : lines.FindIndex(classIndex, l => l.Contains('{'));
                if (braceIndex < 0)
                {
                    Debug.LogError($"Unable to find the declaration of class {idType.Name} in {path}.");
                    return false;
                }

                insertIndex = braceIndex + 1;
                indent = classRegex.Match(lines[classIndex]).Groups[1].Value + "    ";
            }

            lines.Insert(insertIndex, $"{indent}public static readonly {idType.Name} {newId} = new(\"{newId}\");");
            File.WriteAllLines(path, lines);

            var cache = GetCache(idType);
            cache.Ids.Add((StringId)Activator.CreateInstance(idType, newId));
            RebuildOptions(cache);
            AssetDatabase.Refresh();
            return true;
        }

        /// <summary>
        /// Removes the static readonly field holding <paramref name="id"/> from the id's source file.
        /// </summary>
        public static bool DeleteId(Type idType, string id)
        {
            string path = FindScriptPath(idType);
            if (path == null) return false;

            string fieldName = StringId.GetIdFields(idType).FirstOrDefault(f => ((StringId)f.GetValue(null))?.Id == id)?.Name ?? id;
            var lineRegex = new Regex($@"^\s*public\s+static\s+readonly\s+{Regex.Escape(idType.Name)}\s+{Regex.Escape(fieldName)}\s*=");

            var lines = File.ReadAllLines(path).ToList();
            int deleteIndex = lines.FindIndex(l => lineRegex.IsMatch(l));
            if (deleteIndex < 0)
            {
                Debug.LogError($"Unable to find id '{id}' in {path}.");
                return false;
            }

            lines.RemoveAt(deleteIndex);
            File.WriteAllLines(path, lines);

            var cache = GetCache(idType);
            cache.Ids.RemoveAll(x => x.Id == id);
            RebuildOptions(cache);
            AssetDatabase.Refresh();
            return true;
        }

        private static string FindScriptPath(Type idType)
        {
            string fallback = null;
            foreach (string guid in AssetDatabase.FindAssets($"{idType.Name} t:MonoScript", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) != idType.Name) continue;

                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script && script.GetClass() == idType) return path;

                string text = File.ReadAllText(path);
                if (fallback == null && Regex.IsMatch(text, $@"\bclass\s+{Regex.Escape(idType.Name)}\b")
                    && (string.IsNullOrEmpty(idType.Namespace) || text.Contains($"namespace {idType.Namespace}")))
                {
                    fallback = path;
                }
            }

            if (fallback == null)
            {
                Debug.LogError($"Unable to find {idType.Name}.cs file. The file name must match the class name.");
            }

            return fallback;
        }

        #endregion

        #region Cache

        private static IdCache GetCache(Type idType)
        {
            if (Caches.TryGetValue(idType, out var cache)) return cache;

            cache = new IdCache { IdType = idType, Ids = StringId.GetAll(idType).ToList() };
            RebuildOptions(cache);
            Caches[idType] = cache;
            return cache;
        }

        private static void RebuildOptions(IdCache cache)
        {
            cache.Options = new[] { new GUIContent("<None>") }.Concat(cache.Ids.Select(x => new GUIContent(x.Id))).ToArray();
            cache.TakenNames = new HashSet<string>(cache.Ids.Select(x => x.Id));
            foreach (var field in StringId.GetIdFields(cache.IdType))
            {
                cache.TakenNames.Add(field.Name);
            }
        }

        private static string GetStateKey(SerializedProperty property) => $"{property.serializedObject.targetObject.GetEntityId()}:{property.propertyPath}";

        private static bool TryGetState(SerializedProperty property, out FieldState state) => States.TryGetValue(GetStateKey(property), out state);

        private static FieldState GetState(SerializedProperty property)
        {
            string key = GetStateKey(property);
            if (!States.TryGetValue(key, out var state))
            {
                state = new FieldState();
                States[key] = state;
            }

            return state;
        }

        #endregion
    }
}

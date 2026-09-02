using System.IO;
using ReaaliStudio.Utils.Extensions;
using UnityEditor;
using UnityEngine;

namespace CCLBStudio.ScriptableValue
{
    public class ScriptableValueScriptCreatorWindow : EditorWindow
    {
        private string _targetType;
        private bool _createListVersion;
        private ScriptableValueDatabase _database;
        private static readonly string[] DefaultTypes = new[] { "bool", "float", "string", "int", nameof(Vector2), nameof(Vector3), nameof(Transform), nameof(GameObject) };

        private void OnEnable()
        {
            _database = EditorExtender.LoadScriptableAsset<ScriptableValueDatabase>();
        }

        private void OnGUI()
        {
            _targetType = EditorGUILayout.TextField("Target Type", _targetType);
            _createListVersion = EditorGUILayout.Toggle("Create List Version", _createListVersion);

            if (!string.IsNullOrEmpty(_targetType))
            {
                if (GUILayout.Button($"Create New Value Script : {_targetType}"))
                {
                    CreateValueScript(_targetType);

                    if (_createListVersion)
                    {
                        CreateListValueScript(_targetType);
                    }
                }
            }

            if (GUILayout.Button("Create Default Types"))
            {
                CreateDefaultTypes();
            }
        }

        private void CreateDefaultTypes()
        {
            bool anyCreated = false;
            foreach (var typeName in DefaultTypes)
            {
                if (CreateValueScript(typeName))
                {
                    anyCreated = true;
                }

                if (CreateListValueScript(typeName))
                {
                    anyCreated = true;
                }
            }

            if (anyCreated)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private bool CreateValueScript(string typeName, bool autoSaveAndRefresh = true)
        {
            string capitalizedTypeName = char.ToUpper(typeName[0]) + typeName.Substring(1);
            string scriptableTypeName = capitalizedTypeName + "Value";
            string scriptableValuePath = Path.Combine(ScriptableValueDatabase.GetBaseFolderPath(), scriptableTypeName + ".cs");

            if (File.Exists(scriptableValuePath))
            {
                return false;
            }
            
            string content = _database.ScriptableValueTemplate.fileContent.Replace("#NICESCRIPTNAME#", ObjectNames.NicifyVariableName(scriptableTypeName))
                .Replace("#SCRIPTNAME#", scriptableTypeName).Replace("TYPE", typeName);
            File.WriteAllText(scriptableValuePath, content);

            if (autoSaveAndRefresh)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return true;
        }

        private bool CreateListValueScript(string typeName, bool autoSaveAndRefresh = true)
        {
            string capitalizedTypeName = char.ToUpper(typeName[0]) + typeName.Substring(1);
            string scriptableTypeName = capitalizedTypeName + "ListValue";
            string scriptableValuePath = Path.Combine(ScriptableValueDatabase.GetBaseFolderPath(), scriptableTypeName + ".cs");

            if (File.Exists(scriptableValuePath))
            {
                return false;
            }
            
            string content = _database.ScriptableValueListTemplate.fileContent.Replace("#NICESCRIPTNAME#", ObjectNames.NicifyVariableName(scriptableTypeName))
                .Replace("#SCRIPTNAME#", scriptableTypeName).Replace("TYPE", typeName);
            File.WriteAllText(scriptableValuePath, content);

            if (autoSaveAndRefresh)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            return true;
        }

        [MenuItem("Reaali/Scriptable Value Creator")]
        public static void ShowWindow()
        {
            GetWindow<ScriptableValueScriptCreatorWindow>(false, "Scriptable Value Creator", true);
        }
    }
}
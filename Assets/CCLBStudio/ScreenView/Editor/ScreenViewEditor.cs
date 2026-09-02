using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CCLBStudio.EditorTools;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace CCLBStudio.ScreenView
{
    [CustomEditor(typeof(ScreenView), true)]
    public class ScreenViewEditor : Editor
    {
        private SerializedProperty viewId;
        private SerializedProperty textEntries;
        private SerializedProperty imageEntries;
        private SerializedProperty spriteEntries;
        private SerializedProperty gameObjectEntries;
        private SerializedProperty rectTransformEntries;
        private SerializedProperty canvasGroupEntries;
        private SerializedProperty buttonEntries;
        private SerializedProperty entriesSectionExpanded;
        private SerializedProperty animationSectionExpanded;
        private SerializedProperty behaviourSectionExpanded;
        private SerializedProperty mainButtonsSectionExpanded;
        private SerializedProperty displaySectionExpanded;
        private SerializedProperty entriesExpanded;
        private SerializedProperty onShowCompleted;
        private SerializedProperty onCloseCompleted;
        private SerializedProperty onClose;
        private SerializedProperty onShow;
        private SerializedProperty deactivateBtnOnSubmit;
        private SerializedProperty autoCloseOnSubmit;
        private SerializedProperty autoCloseOnCancel;
        private SerializedProperty submitButtons;
        private SerializedProperty cancelButtons;
        private SerializedProperty closeButtons;
        private SerializedProperty customButtons;
        private SerializedProperty closeBehaviour;
        private SerializedProperty displayBehaviour;
        private SerializedProperty uiContainerIndex;
        private SerializedProperty showAnimations;
        private SerializedProperty closeAnimations;
        private SerializedProperty containerId;
        private SerializedProperty buttonFeedbacksExpanded;
        private SerializedProperty optiSectionExpanded;
        private SerializedProperty enableOpti;
        private SerializedProperty updateTargetFrameRate;
        private SerializedProperty newTargetFrameRate;
        private SerializedProperty disableLowerCanvas;
        private SerializedProperty disableMainCam;
        private SerializedProperty blockRaycastUntilCloseCompleted;
        private SerializedProperty displayShow, displayClose;
        private SerializedProperty buttonFeedbacks;
        
        private ReorderableList _showAnimationList;
        private ReorderableList _closeAnimationList;
        private List<Type> _concreteTypes;
        private bool _creatingNewId;
        private string _newId = "";
        private readonly List<ViewId> _allIds = ViewId.GetAll().ToList();
        private bool _focusIdNextFrame;
        private const string NewIdFocusName = "NewIdTextField";
        private GUIStyle _sectionStyle, _blueStyle;
        private bool _stylesInit;
        private readonly Color _sectionFoldoutTint = new Color(0.5f, 0.5f, 0.5f, 1f);

        private ScreenViewService service;

        // ReSharper disable once HeapView.ObjectAllocation
        private readonly string[] toExclude = 
        {
            ScreenView.ViewIdProperty, ScreenView.TextEntriesProperty, ScreenView.ImageEntriesProperty, ScreenView.SpriteEntriesProperty, ScreenView.GameObjectEntriesProperty, ScreenView.RectTransformEntriesProperty, ScreenView.ButtonEntriesProperty, ScreenView.CanvasGroupEntriesProperty,
            ScreenView.EntriesSectionExpandedProperty, ScreenView.DisableInteractionOnProperty, ScreenView.AutoCloseOnCancelProperty, ScreenView.AutoCloseOnSubmitProperty, ScreenView.ButtonFeedbacksProperty,
            ScreenView.AnimationSectionExpandedProperty, ScreenView.DisplayShowProperty, ScreenView.DisplayCloseProperty, ScreenView.DisableAndCloseSectionExpandedProperty, ScreenView.MainButtonsSectionExpandedProperty, ScreenView.SubmitButtonsProperty, ScreenView.CancelButtonsProperty,
            ScreenView.CloseButtonsProperty, ScreenView.CustomButtonsProperty, ScreenView.CloseBehaviourProperty, ScreenView.DisplaySectionExpandedProperty, ScreenView.DisplayBehaviourProperty, ScreenView.ButtonFeedbacksSectionExpandedProperty,
            ScreenView.ContainerIndexProperty, ScreenView.ContainerIdProperty, ScreenView.OnShowAnimCompletedProperty, ScreenView.OnCloseAnimCompletedProperty, ScreenView.ShowAnimationProperty, ScreenView.CloseAnimationProperty,
            ScreenView.OptiSectionExpandedProperty, ScreenView.EnableOptiProperty, ScreenView.UpdateTargetFrameRateProperty, ScreenView.NewTargetFrameRateProperty, ScreenView.DisableLowerCanvasProperty, ScreenView.DisableMainCamProperty, ScreenView.OnShowProperty, ScreenView.OnCloseProperty, ScreenView.BlockRaycastUntilCloseCompletedProperty
        };

        
        private void OnEnable()
        {
            viewId = serializedObject.FindProperty(ScreenView.ViewIdProperty);
            textEntries = serializedObject.FindProperty(ScreenView.TextEntriesProperty);
            imageEntries = serializedObject.FindProperty(ScreenView.ImageEntriesProperty);
            spriteEntries = serializedObject.FindProperty(ScreenView.SpriteEntriesProperty);
            canvasGroupEntries = serializedObject.FindProperty(ScreenView.CanvasGroupEntriesProperty);
            gameObjectEntries = serializedObject.FindProperty(ScreenView.GameObjectEntriesProperty);
            rectTransformEntries = serializedObject.FindProperty(ScreenView.RectTransformEntriesProperty);
            buttonEntries = serializedObject.FindProperty(ScreenView.ButtonEntriesProperty);
            entriesExpanded = serializedObject.FindProperty(ScreenView.EntriesSectionExpandedProperty);
            animationSectionExpanded = serializedObject.FindProperty(ScreenView.AnimationSectionExpandedProperty);
            displayShow = serializedObject.FindProperty(ScreenView.DisplayShowProperty);
            displayClose = serializedObject.FindProperty(ScreenView.DisplayCloseProperty);
            behaviourSectionExpanded = serializedObject.FindProperty(ScreenView.DisableAndCloseSectionExpandedProperty);
            mainButtonsSectionExpanded = serializedObject.FindProperty(ScreenView.MainButtonsSectionExpandedProperty);
            displaySectionExpanded = serializedObject.FindProperty(ScreenView.DisplaySectionExpandedProperty);
            buttonFeedbacksExpanded = serializedObject.FindProperty(ScreenView.ButtonFeedbacksSectionExpandedProperty);
            deactivateBtnOnSubmit = serializedObject.FindProperty(ScreenView.DisableInteractionOnProperty);
            autoCloseOnCancel = serializedObject.FindProperty(ScreenView.AutoCloseOnCancelProperty);
            autoCloseOnSubmit = serializedObject.FindProperty(ScreenView.AutoCloseOnSubmitProperty);
            submitButtons = serializedObject.FindProperty(ScreenView.SubmitButtonsProperty);
            cancelButtons = serializedObject.FindProperty(ScreenView.CancelButtonsProperty);
            closeButtons = serializedObject.FindProperty(ScreenView.CloseButtonsProperty);
            customButtons = serializedObject.FindProperty(ScreenView.CustomButtonsProperty);
            closeBehaviour = serializedObject.FindProperty(ScreenView.CloseBehaviourProperty);
            displayBehaviour = serializedObject.FindProperty(ScreenView.DisplayBehaviourProperty);
            uiContainerIndex = serializedObject.FindProperty(ScreenView.ContainerIndexProperty);
            showAnimations = serializedObject.FindProperty(ScreenView.ShowAnimationProperty);
            closeAnimations = serializedObject.FindProperty(ScreenView.CloseAnimationProperty);
            containerId = serializedObject.FindProperty(ScreenView.ContainerIdProperty);
            onShowCompleted = serializedObject.FindProperty(ScreenView.OnShowAnimCompletedProperty);
            onShow = serializedObject.FindProperty(ScreenView.OnShowProperty);
            onCloseCompleted = serializedObject.FindProperty(ScreenView.OnCloseAnimCompletedProperty);
            onClose = serializedObject.FindProperty(ScreenView.OnCloseProperty);
            optiSectionExpanded = serializedObject.FindProperty(ScreenView.OptiSectionExpandedProperty);
            enableOpti = serializedObject.FindProperty(ScreenView.EnableOptiProperty);
            updateTargetFrameRate = serializedObject.FindProperty(ScreenView.UpdateTargetFrameRateProperty);
            newTargetFrameRate = serializedObject.FindProperty(ScreenView.NewTargetFrameRateProperty);
            disableLowerCanvas = serializedObject.FindProperty(ScreenView.DisableLowerCanvasProperty);
            disableMainCam = serializedObject.FindProperty(ScreenView.DisableMainCamProperty);
            blockRaycastUntilCloseCompleted = serializedObject.FindProperty(ScreenView.BlockRaycastUntilCloseCompletedProperty);
            buttonFeedbacks = serializedObject.FindProperty(ScreenView.ButtonFeedbacksProperty);

            BuildAnimationLists();
        }

        private void BuildAnimationLists()
        {
            _concreteTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => typeof(IScreenViewAnimation).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                .ToList();
            
            _showAnimationList = new ReorderableList(serializedObject, showAnimations, true, false, true, true);
            _closeAnimationList = new ReorderableList(serializedObject, closeAnimations, true, false, true, true);

            _showAnimationList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = showAnimations.GetArrayElementAtIndex(index);
                rect.x += 10;
                rect.width -= 10;
                EditorGUI.PropertyField(rect, element, new GUIContent($"Element {index}"), true);
            };
            _closeAnimationList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var element = closeAnimations.GetArrayElementAtIndex(index);
                rect.x += 10;
                rect.width -= 10;
                EditorGUI.PropertyField(rect, element, new GUIContent($"Element {index}"), true);
            };

            _showAnimationList.elementHeightCallback = (int index) =>
            {
                var element = showAnimations.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 4;
            };
            _closeAnimationList.elementHeightCallback = (int index) =>
            {
                var element = closeAnimations.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + 4;
            };

            _showAnimationList.onAddDropdownCallback = (Rect buttonRect, ReorderableList l) =>
            {
                AddElementToList(showAnimations);
            };
            _closeAnimationList.onAddDropdownCallback = (_, _) =>
            {
                AddElementToList(closeAnimations);
            };
        }

        private void AddElementToList(SerializedProperty list)
        {
            var menu = new GenericMenu();

            foreach (var type in _concreteTypes)
            {
                string typeName = ObjectNames.NicifyVariableName(type.Name);
                menu.AddItem(new GUIContent(typeName), false, () =>
                {
                    var obj = Activator.CreateInstance(type);
                        
                    list.arraySize++;
                    var newElement = list.GetArrayElementAtIndex(list.arraySize - 1);
                    newElement.managedReferenceValue = obj;
                        
                    serializedObject.ApplyModifiedProperties();
                        
                    var updatedElement = list.GetArrayElementAtIndex(list.arraySize - 1);
                    var managedObj = updatedElement.managedReferenceValue as IScreenViewAnimation;
                    managedObj?.OnEditorCreated((ScreenView)target);
                        
                    serializedObject.ApplyModifiedProperties();
                });
            }

            menu.ShowAsContext();
        }

        private void InitStyles()
        {
            _stylesInit = true;
            _sectionStyle = new GUIStyle(EditorStyles.foldoutHeader) { normal = { textColor = Color.yellow }, onNormal = { textColor = Color.yellow }, focused = { textColor = Color.yellow }, onFocused = { textColor = Color.yellow }, fontStyle = FontStyle.Bold,};
            _blueStyle = EditorExtender.URLButtonStyle;
        }

        private bool DrawSectionFoldout(SerializedProperty expanded, string title)
        {
            Color col = GUI.backgroundColor;
            float labelWidth = EditorGUIUtility.labelWidth;
            
            GUI.backgroundColor = _sectionFoldoutTint;
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth - 12;
            expanded.boolValue = EditorGUILayout.Foldout(expanded.boolValue, title, true, _sectionStyle);
            GUI.backgroundColor = col;
            EditorGUIUtility.labelWidth = labelWidth;
            return expanded.boolValue;
        }

        public override void OnInspectorGUI()
        {
            if(!_stylesInit) InitStyles();
            
            serializedObject.Update();
            ScreenView script = (ScreenView)target;
            if (!service)
            {
                service = EditorExtender.LoadScriptableAsset<ScreenViewService>();
            }

            DrawHeaderSection(script);
            DrawId();
            DrawEntriesSection();
            DrawMainButtonsSections();
            DrawDisplaySettingsSection();
            DrawDisableAndCloseSection();
            DrawAnimationSection();
            DrawButtonFeedbacksSection();
            DrawOptimizationSection();

            EditorGUILayout.Space(10);
            EditorExtender.DrawFullHierarchyPropertiesExcluding(serializedObject, false, toExclude);

            serializedObject.ApplyModifiedProperties();
        }

        #region Sections Draw Methods

        private void DrawId()
        {
            var idProperty = viewId.FindPropertyRelative(ViewId.IdProperty);
            var currentId = new ViewId(idProperty.stringValue);

            int currentIndex = Mathf.Max(0, _allIds.FindIndex(vid => vid == currentId));
            var names = _allIds.Select(vid => vid.ToString()).ToArray();

            if (currentId == ViewId.Empty)
            {
                EditorGUILayout.HelpBox("A valid ID is required", MessageType.Error);
            }

            EditorGUILayout.BeginHorizontal();
            
            int newIndex = EditorGUILayout.Popup("Id", currentIndex, names, GUILayout.Width(EditorGUIUtility.currentViewWidth - 100));
            if (currentId != ViewId.Empty && GUILayout.Button("Delete"))
            {
                DeleteViewId(currentId.Id);
                idProperty.stringValue = string.Empty;
                EditorGUILayout.EndHorizontal();
                return;
            }
            
            EditorGUILayout.EndHorizontal();

            if (newIndex != currentIndex)
            {
                idProperty.stringValue = _allIds[newIndex].Id;
            }
            
            if (_creatingNewId)
            {
                EditorGUILayout.BeginHorizontal();
                
                GUI.SetNextControlName(NewIdFocusName);
                _newId = EditorGUILayout.TextField(_newId).Capitalize();
                bool valid = true;
                bool alreadyExists = _allIds.Any(x => x.Id == _newId);
                
                if (_focusIdNextFrame && Event.current.type == EventType.Repaint)
                {
                    GUI.FocusControl(NewIdFocusName);
                    _focusIdNextFrame = false;
                }
                
                if (_newId.Length < 1)
                {
                    valid = false;
                }
                
                if (alreadyExists)
                {
                    valid = false;
                }

                GUI.enabled = valid;
                
                if (GUILayout.Button("Add", GUILayout.Width(50)))
                {
                    AddNewViewId(_newId);
                    idProperty.stringValue = _newId;
                    _creatingNewId = false;
                    _newId = string.Empty;
                    GUI.FocusControl(null);
                }
                
                GUI.enabled = true;
                
                EditorGUILayout.EndHorizontal();

                if (alreadyExists && _newId.Length > 1)
                {
                    EditorGUILayout.HelpBox("Id already exists", MessageType.Error);
                }
            }
            else if (GUILayout.Button("New Id"))
            {
                _creatingNewId = true;
                _focusIdNextFrame = true;
            }
        }
        
        private void AddNewViewId(string newId)
        {
            string[] guids = AssetDatabase.FindAssets($"{nameof(ViewId)} t:script", new[] {"Assets"});
            string path = string.Empty;
            foreach (string guid in guids)
            {
                path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == nameof(ViewId))
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"Unable to find {nameof(ViewId)}.cs file.");
                return;
            }
            
            string[] lines = File.ReadAllLines(path);

            int insertIndex = Array.FindLastIndex(lines, l => l.Trim().StartsWith("public static readonly ViewId")) + 1;

            if (insertIndex <= 0) return;

            string newEntry = $"        public static readonly ViewId {newId} = new(\"{newId}\");";

            var newLines = lines.Take(insertIndex)
                .Concat(new[] { newEntry })
                .Concat(lines.Skip(insertIndex))
                .ToArray();

            File.WriteAllLines(path, newLines);
            AssetDatabase.Refresh();
        }

        private void DeleteViewId(string id)
        {
            string[] guids = AssetDatabase.FindAssets($"{nameof(ViewId)} t:script", new[] {"Assets"});
            string path = string.Empty;
            foreach (string guid in guids)
            {
                path = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(path) == nameof(ViewId))
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"Unable to find {nameof(ViewId)}.cs file.");
                return;
            }
            
            string[] lines = File.ReadAllLines(path);
            int deleteIndex = Array.FindIndex(lines, l => l.Trim().StartsWith($"public static readonly ViewId {id}"));
            if (deleteIndex < 0) return;
            
            int index = _allIds.FindIndex(x => x.Id == id);
            _allIds.RemoveAt(index);
            
            var newLines = lines.Take(deleteIndex)
                .Concat(lines.Skip(deleteIndex + 1))
                .ToArray();
            
            File.WriteAllLines(path, newLines);
            AssetDatabase.Refresh();
        }

        private void DrawHeaderSection(ScreenView script)
        {
            EditorExtender.DrawScriptField(serializedObject);
            if (GUILayout.Button("UI Good Practices", _blueStyle))
            {
                Application.OpenURL("https://unity.com/how-to/unity-ui-optimization-tips");
            }
            if (GUILayout.Button("Easing Functions", _blueStyle))
            {
                Application.OpenURL("https://easings.net/");
            }
            
            GUILayout.Space(5);
        }

        private void DrawEntriesSection()
        {
            if (!DrawSectionFoldout(entriesExpanded, "Entries"))
            {
                return;
            }
            
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(textEntries);
            EditorGUILayout.PropertyField(canvasGroupEntries);
            EditorGUILayout.PropertyField(imageEntries);
            EditorGUILayout.PropertyField(spriteEntries);
            EditorGUILayout.PropertyField(gameObjectEntries);
            EditorGUILayout.PropertyField(rectTransformEntries);
            EditorGUILayout.PropertyField(buttonEntries, new GUIContent("Button Entries"));
            EditorGUI.indentLevel--;
        }

        private void DrawAnimationSection()
        {
            EditorGUILayout.Space(5);
            if (!DrawSectionFoldout(animationSectionExpanded, "Animations"))
            {
                return;
            }
            
            EditorGUI.indentLevel++;
            
            displayShow.boolValue = EditorGUILayout.Foldout(displayShow.boolValue, "Show Animations", true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
            if(displayShow.boolValue)
            {
                _showAnimationList.DoLayoutList();
            }
            
            displayClose.boolValue = EditorGUILayout.Foldout(displayClose.boolValue, "Close Animations", true, new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold });
            if(displayClose.boolValue)
            {
                _closeAnimationList.DoLayoutList();
            }

            GUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            EditorGUILayout.PropertyField(onShow);
            EditorGUILayout.PropertyField(onShowCompleted);
            EditorGUILayout.PropertyField(onClose);
            EditorGUILayout.PropertyField(onCloseCompleted);
            
            EditorGUI.indentLevel--;
        }

        private void DrawMainButtonsSections()
        {
            EditorGUILayout.Space(5);
            if (!DrawSectionFoldout(mainButtonsSectionExpanded, "Main Buttons"))
            {
                return;
            }
            
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(submitButtons);
            EditorGUILayout.PropertyField(cancelButtons);
            EditorGUILayout.PropertyField(closeButtons);
            EditorGUILayout.PropertyField(customButtons);
            EditorGUI.indentLevel--;
        }

        private void DrawDisableAndCloseSection()
        {
            EditorGUILayout.Space(5);
            if (!DrawSectionFoldout(behaviourSectionExpanded, "Disable/Close"))
            {
                return;
            }
            
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(deactivateBtnOnSubmit);
            EditorGUILayout.PropertyField(autoCloseOnSubmit);
            EditorGUILayout.PropertyField(autoCloseOnCancel);
            EditorGUILayout.PropertyField(blockRaycastUntilCloseCompleted);
            EditorGUILayout.PropertyField(closeBehaviour);
            EditorGUI.indentLevel--;
        }

        private void DrawDisplaySettingsSection()
        {
            EditorGUILayout.Space(5);
            if (!DrawSectionFoldout(displaySectionExpanded, "Display"))
            {
                return;
            }
            
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(displayBehaviour);
            ShowBehaviour behaviour = (ShowBehaviour)displayBehaviour.enumValueIndex;
            if (behaviour == ShowBehaviour.InsideContainer || behaviour == ShowBehaviour.InsideContainerAsLastSibling)
            {
                var keys = service.ContainerIds.Where(x => !string.IsNullOrEmpty(x)).ToList();
                if (keys.Count > 0)
                {
                    int index = keys.FindIndex(x => x.Equals(containerId.stringValue));
                    uiContainerIndex.intValue = index >= 0 ? index : Mathf.Clamp(uiContainerIndex.intValue, 0, keys.Count - 1);
                    uiContainerIndex.intValue = EditorExtender.DrawPopup(uiContainerIndex.intValue, keys.ToArray(), "Container Id");
                    containerId.stringValue = keys[uiContainerIndex.intValue];
                }
                else
                {
                    containerId.stringValue = string.Empty;
                }
            }

            EditorGUI.indentLevel--;
        }
        
        private void DrawButtonFeedbacksSection()
        {
            EditorGUILayout.Space(5);
            if (!DrawSectionFoldout(buttonFeedbacksExpanded, "Button Feedbacks"))
            {
                return;
            }

            if (GUILayout.Button("Find All Buttons", _blueStyle))
            {
                var script = (ScreenView)target;
                var buttons = script.GetComponentsInChildren<Button>().ToList();

                for (int i = 0; i < buttonFeedbacks.arraySize; i++)
                {
                    var buttonProp = buttonFeedbacks.GetArrayElementAtIndex(i).FindPropertyRelative(ScreenViewButtonFeedback.TargetProperty);
                    if (!buttonProp.objectReferenceValue)
                    {
                        continue;
                    }
                    
                    var btn = buttonProp.objectReferenceValue as Button;
                    int index = buttons.FindIndex(x => x.Equals(btn));
                    if (index >= 0)
                    {
                        buttons.RemoveAt(index);
                    }
                }

                foreach (var btn in buttons)
                {
                    var obj = new ScreenViewButtonFeedback();
                    obj.SetButtonEditor(btn);
                    
                    buttonFeedbacks.arraySize++;
                    var newElement = buttonFeedbacks.GetArrayElementAtIndex(buttonFeedbacks.arraySize - 1);
                    newElement.FindPropertyRelative(ScreenViewButtonFeedback.TargetProperty).objectReferenceValue = btn;
                }
            }
            
            EditorGUILayout.PropertyField(buttonFeedbacks);
        }

        private void DrawOptimizationSection()
        {
            EditorGUILayout.Space(5);
            if (!DrawSectionFoldout(optiSectionExpanded, "Optimizations"))
            {
                return;
            }
            
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(enableOpti);
            if (!enableOpti.boolValue)
            {
                EditorGUI.indentLevel--;
                return;
            }
            
            EditorGUILayout.PropertyField(updateTargetFrameRate);
            if (updateTargetFrameRate.boolValue)
            {
                EditorGUILayout.PropertyField(newTargetFrameRate);
            }

            EditorGUILayout.PropertyField(disableLowerCanvas);
            EditorGUILayout.PropertyField(disableMainCam);
            
            EditorGUI.indentLevel--;
        }

        #endregion
    }
}
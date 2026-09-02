using System;
using System.Collections.Generic;
using System.Linq;
using CCLBStudio.EditorTools;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCLBStudio.ScreenView
{
    [CustomEditor(typeof(ScreenViewService))]
    public class ScreenViewServiceEditor : Editor
    {
        private SerializedProperty _screenViewPrefabList;
        private SerializedProperty _uiContainers;
        private List<SerializedProperty> _hierarchyProperties;

        private GUIStyle _searchStyle;
        private ScreenViewService _script;
        private Dictionary<int, int> _filteredToGlobalIndex = new();
        private List<GameObject> _filteredPrefabs = new List<GameObject>();

        private ReorderableList _viewPrefabReorderableList;
        private string _searchFilter;
        [NonSerialized] private bool _init;

        private void OnEnable()
        {
            _screenViewPrefabList = serializedObject.FindProperty(ScreenViewService.ScreenViewPrefabsProperty);
            _uiContainers = serializedObject.FindProperty(ScreenViewService.UiContainersProperty);
            
            _script = (ScreenViewService)target;
            _searchFilter = string.Empty;

            _viewPrefabReorderableList = new ReorderableList(serializedObject, _screenViewPrefabList, true, false, true, true)
            {
                list = _script.ScreenViewPrefabs,
                drawElementCallback = DrawViewListElement,
                onAddCallback = AddElementCallback,
                onRemoveCallback = RemoveElementCallback
            };
            
            _hierarchyProperties = EditorExtender.GetFullHierarchyPropertiesExcluding(serializedObject, ScreenViewService.ScreenViewPrefabsProperty, ScreenViewService.UiContainersProperty);
        }
    
        public override void OnInspectorGUI()
        {
            if (!_init)
            {
                SetupStyles();
                _init = true;
            }
            
            EditorExtender.DrawScriptField(serializedObject);
            serializedObject.Update();
            
            DrawViewPrefabList();
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_uiContainers);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            
            EditorGUI.BeginChangeCheck();

            foreach (var p in _hierarchyProperties)
            {
                EditorGUILayout.PropertyField(p);
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void SetupStyles()
        {
            _searchStyle = GUI.skin.FindStyle("ToolbarSearchTextField");
        }

        #region Filtering Methods

        private void UpdateFilteredList()
        {
            _filteredPrefabs.Clear();
            _filteredToGlobalIndex.Clear();
            
            if (string.IsNullOrEmpty(_searchFilter))
            {
                return;
            }

            for (int i = 0; i < _script.ScreenViewPrefabs.Count; i++)
            {
                GameObject prefab = _script.ScreenViewPrefabs[i];
                if (!prefab || !prefab.TryGetComponent(out ScreenView sv))
                {
                    continue;
                }

                if (sv.GetId().Id.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
                {
                    _filteredToGlobalIndex[_filteredPrefabs.Count] = i;
                    _filteredPrefabs.Add(prefab);
                }
            }
            
            // _filteredPrefabs = new List<GameObject>(_script.ScreenViewPrefabs.Count);
            // foreach (var prefab in _script.ScreenViewPrefabs)
            // {
            //     if (!prefab || !prefab.TryGetComponent(out ScreenView sv))
            //     {
            //         continue;
            //     }
            //
            //     if (sv.GetId().Contains(_searchFilter, StringComparison.OrdinalIgnoreCase))
            //     {
            //         _filteredPrefabs.Add(prefab);
            //     }
            // }
        }

        private void RefreshFilteredData()
        {
            UpdateFilteredList();
            UpdateReorderableList();
        }

        #endregion
        
        #region Reorderable List Methods

        private void UpdateReorderableList()
        {
            if (string.IsNullOrEmpty(_searchFilter))
            {
                _viewPrefabReorderableList = new ReorderableList(serializedObject, _screenViewPrefabList, true, false, true, true)
                {
                    list = _script.ScreenViewPrefabs,
                    drawElementCallback = DrawViewListElement,
                    onAddCallback = AddElementCallback,
                    onRemoveCallback = RemoveElementCallback
                };
                return;
                
            }
            _viewPrefabReorderableList = new ReorderableList(_filteredPrefabs, typeof(GameObject), true, false, true, true)
            {
                drawElementCallback = DrawViewListElement,
                onAddCallback = AddElementCallback,
                onRemoveCallback = RemoveElementCallback
            };
        }

        private void DrawViewPrefabList()
        {
            EditorGUILayout.LabelField("Main Objects", EditorStyles.boldLabel);
            _screenViewPrefabList.isExpanded = EditorGUILayout.Foldout(_screenViewPrefabList.isExpanded, "View Prefabs", true, EditorStyles.foldoutHeader);
            HandleDragAndDrop();
            
            if (_screenViewPrefabList.isExpanded)
            {
                GUILayout.BeginHorizontal();
                EditorGUI.BeginChangeCheck();
                _searchFilter = EditorGUILayout.TextField(_searchFilter, _searchStyle, GUILayout.Width(EditorGUIUtility.labelWidth));
                if (EditorGUI.EndChangeCheck())
                {
                    RefreshFilteredData();
                }
                
                EditorGUI.BeginChangeCheck();
                GUILayout.FlexibleSpace();
                int size = EditorGUILayout.DelayedIntField(_screenViewPrefabList.arraySize, GUILayout.Width(50));
                if (EditorGUI.EndChangeCheck())
                {
                    size = Mathf.Max(0, size);
                    _screenViewPrefabList.arraySize = size;
                    serializedObject.ApplyModifiedProperties();
                    RefreshFilteredData();
                }
                GUILayout.EndHorizontal();
                
                _viewPrefabReorderableList.DoLayoutList();
            }
        }

        private void DrawViewListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            float diff = rect.height * .1f;
            rect.height -= diff;
            rect.y += diff / 2f;

            int globalIndex = index;
            if (!string.IsNullOrEmpty(_searchFilter) && _filteredToGlobalIndex.TryGetValue(index, out int realIndex))
            {
                globalIndex = realIndex;
            }

            SerializedProperty elementProperty = _screenViewPrefabList.GetArrayElementAtIndex(globalIndex);
            GameObject elem = elementProperty.objectReferenceValue as GameObject;
            ScreenView sv = elem ? elem.GetComponent<ScreenView>() : null;

            EditorGUI.LabelField(rect, elem ? sv ? sv.GetId().Id : "!!! No ScreenView component !!!" : "Empty Element");
            
            rect.x += EditorGUIUtility.labelWidth;
            rect.width -= EditorGUIUtility.labelWidth;
            
            EditorGUI.BeginChangeCheck();
            var newObj = (GameObject)EditorGUI.ObjectField(rect, elem, typeof(GameObject), false);
            if (EditorGUI.EndChangeCheck())
            {
                elementProperty.objectReferenceValue = newObj;
                serializedObject.ApplyModifiedProperties();
                
                RefreshFilteredData();
            }
        }

        private void AddElementCallback(ReorderableList reorderableList)
        {
            _screenViewPrefabList.arraySize++;
            _screenViewPrefabList.GetArrayElementAtIndex(_screenViewPrefabList.arraySize - 1).objectReferenceValue = null;
            serializedObject.ApplyModifiedProperties();
        }

        private void RemoveElementCallback(ReorderableList reorderableList)
        {
            int i = string.IsNullOrEmpty(_searchFilter) ? reorderableList.index : _filteredToGlobalIndex[reorderableList.index];
            _script.ScreenViewPrefabs.RemoveAt(i);
            serializedObject.ApplyModifiedProperties();
            RefreshFilteredData();
        }

        private void HandleDragAndDrop()
        {
            Event evt = Event.current;
            Rect dropArea = GUILayoutUtility.GetLastRect();
            if (!dropArea.Contains(evt.mousePosition))
            {
                return;
            }
            
            switch (evt.type)
            {
                case EventType.Repaint:
                case EventType.DragUpdated:
                    if (DragAndDrop.objectReferences.Length > 0 && DragAndDrop.objectReferences.All(obj => obj is GameObject))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if(evt.type == EventType.DragUpdated)
                        {
                            Event.current.Use();
                        }
                    }
                    break;

                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();
                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        if (obj is GameObject go)
                        {
                            AddElementToList(go);
                        }
                    }
                    Event.current.Use();
                    serializedObject.ApplyModifiedProperties();
                    RefreshFilteredData();
                    break;
                
                case EventType.DragExited:
                    DragAndDrop.visualMode = DragAndDropVisualMode.None;
                    Event.current.Use();
                    break;
            }
        }
        
        private void AddElementToList(GameObject go)
        {
            int index = _screenViewPrefabList.arraySize;
            _screenViewPrefabList.arraySize++;
            _screenViewPrefabList.GetArrayElementAtIndex(index).objectReferenceValue = go;
        }

        #endregion
    }
}

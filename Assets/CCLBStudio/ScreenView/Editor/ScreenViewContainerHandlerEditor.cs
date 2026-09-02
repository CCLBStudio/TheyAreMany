using System.Collections.Generic;
using CCLBStudio.EditorTools;
using UnityEditor;
using UnityEngine;

namespace CCLBStudio.ScreenView
{
    [CustomEditor(typeof(ScreenViewContainerHandler), true)]
    public class ScreenViewContainerHandlerEditor : Editor
    {
        private SerializedProperty canvas;
        private SerializedProperty container;
        private bool _displaySvList;
        private ScreenViewService _screenViewService;
        private List<ScreenView> _relatedScreenViews;

        private void OnEnable()
        {
            canvas = serializedObject.FindProperty(ScreenViewContainerHandler.CanvasProperty);
            container = serializedObject.FindProperty(ScreenViewContainerHandler.ContainerProperty);
            _screenViewService = EditorExtender.LoadScriptableAsset<ScreenViewService>();

            var map = _screenViewService.GetContainersViewMapEditor();
            map.TryGetValue((ScreenViewContainerHandler)target, out _relatedScreenViews);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            
            DrawScreenViewList();

            container.objectReferenceValue = container.objectReferenceValue ? container.objectReferenceValue : ((Canvas)canvas.objectReferenceValue).GetComponent<RectTransform>();
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScreenViewList()
        {
            if (_relatedScreenViews == null)
            {
                return;
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Related Screen Views:", EditorStyles.boldLabel);
            
            EditorGUI.indentLevel++;
            foreach (var view in _relatedScreenViews)
            {
                GUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField(view.GetId().ToString());
                EditorGUILayout.ObjectField(view.gameObject, typeof(GameObject), false);
                
                GUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel--;
        }
    }
}
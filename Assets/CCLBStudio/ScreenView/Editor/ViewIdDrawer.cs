using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CCLBStudio.ScreenView
{
    [CustomPropertyDrawer(typeof(ViewId))]
    public class ViewIdDrawer : PropertyDrawer
    {
        private string[] _options;
        private ViewId[] _viewIds;
        private SerializedProperty _id;
        private string _initFor;
        private int _currentIndex;
        
        private void Initialize(SerializedProperty property)
        {
            _initFor = property.propertyPath;
            _viewIds = ViewId.GetAll().ToArray();
            _options = _viewIds.Select(v => v.ToString()).ToArray();
            _id = property.FindPropertyRelative(ViewId.IdProperty);
            
            _currentIndex = Array.FindIndex(_viewIds, v => v.Id == _id.stringValue);
            if (_currentIndex == -1)
            {
                _currentIndex = 0;
            }
        }
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(_initFor) || _initFor != property.propertyPath)
            {
                Initialize(property);
            }
            
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.BeginChangeCheck();
            _currentIndex = EditorGUI.Popup(position, label.text, _currentIndex, _options);
            if (EditorGUI.EndChangeCheck())
            {
                _id.stringValue = _viewIds[_currentIndex].Id;
            }
            EditorGUI.EndProperty();
        }
    }
}

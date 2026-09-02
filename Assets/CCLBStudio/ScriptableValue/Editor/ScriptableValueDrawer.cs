using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CCLBStudio.ScriptableValue
{
    [CustomPropertyDrawer(typeof(ScriptableValue<>), true)]
    public class ScriptableValueDrawer : PropertyDrawer
    {
        private readonly Dictionary<string, DrawerProperties> _initializedDrawers = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var properties = Initialize(property);

            var value = (BaseScriptableValue)property.objectReferenceValue;
            float availableWidth = position.width;

            float width = availableWidth - 110f;
            position.width = width;
            properties.secondaryRect.x = position.x + position.width + 10;
            properties.secondaryRect.y = position.y;
            properties.secondaryRect.height = position.height;
            
            EditorGUI.PropertyField(position, property, label);

            if (!value)
            {
                properties.secondaryRect.width = 100f;

                if (GUI.Button(properties.secondaryRect, "Create New"))
                {
                    Type type = fieldInfo.FieldType;
                    ScriptableObject asset = ScriptableValueDatabase.CreateValueAsset(type, string.Empty);
                    property.objectReferenceValue = asset;
                }
            }
            else if(!value.TargetTypeIsList && property.objectReferenceValue)
            {
                GUI.enabled = !Application.isPlaying;
                
                properties.secondaryRect.width = 200f;
                properties.valueSerializedObject ??= new SerializedObject(property.objectReferenceValue);
                SerializedProperty valueProperty = properties.valueSerializedObject.FindProperty(ScriptableValue<dynamic>.ValueProperty);
                EditorGUI.PropertyField(properties.secondaryRect, valueProperty, new GUIContent(""));
                properties.valueSerializedObject.ApplyModifiedProperties();
                
                GUI.enabled = true;
            }
        }

        private DrawerProperties Initialize(SerializedProperty property)
        {
            string drawerKey = property.propertyPath;
            if (_initializedDrawers.TryGetValue(drawerKey, out var properties))
            {
                return properties;
            }

            var drawerProperties = new DrawerProperties
            {
                secondaryRect = new Rect()
            };

            _initializedDrawers.Add(drawerKey, drawerProperties);
            return drawerProperties;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
        
        private struct DrawerProperties
        {
            public SerializedObject valueSerializedObject;
            public Rect secondaryRect;
        }
    }
}
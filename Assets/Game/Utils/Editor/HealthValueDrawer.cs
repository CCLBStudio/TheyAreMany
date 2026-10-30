using System.Collections.Generic;
using CCLBStudio.ScriptableValue;
using UnityEditor;
using UnityEngine;

namespace Game.Utils
{
    [CustomPropertyDrawer(typeof(HealthValue))]
    public class HealthValueDrawer : PropertyDrawer
    {
        private const float ButtonWidth = 120f;
        private const float ButtonSpacing = 10f;

        private readonly Dictionary<string, SerializedObject> _valueObjects = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float lineStep = lineHeight + EditorGUIUtility.standardVerticalSpacing;
            var healthValue = property.objectReferenceValue as HealthValue;

            var objectRect = new Rect(position.x, position.y, position.width, lineHeight);
            if (!healthValue)
            {
                objectRect.width -= ButtonWidth + ButtonSpacing;
                var buttonRect = new Rect(objectRect.xMax + ButtonSpacing, position.y, ButtonWidth, lineHeight);
                if (GUI.Button(buttonRect, "Create New"))
                {
                    property.objectReferenceValue = ScriptableValueDatabase.CreateValueAsset(typeof(HealthValue), string.Empty);
                }
            }

            EditorGUI.PropertyField(objectRect, property, label);

            if (healthValue && property.objectReferenceValue == healthValue)
            {
                SerializedObject valueObject = GetValueObject(property, healthValue);
                valueObject.Update();

                SerializedProperty maxValueProperty = valueObject.FindProperty(HealthValue.MaxValueProperty);
                SerializedProperty valueProperty = valueObject.FindProperty(ScriptableValue<float>.ValueProperty);
                SerializedProperty resetProperty = valueObject.FindProperty(ScriptableValue<float>.ResetValueOnPlayModeExitProperty);

                EditorGUI.indentLevel++;

                var maxValueRect = new Rect(position.x, position.y + lineStep, position.width, lineHeight);
                EditorGUI.PropertyField(maxValueRect, maxValueProperty);

                bool previousEnabled = GUI.enabled;
                GUI.enabled = previousEnabled && !(Application.isPlaying && resetProperty.boolValue);
                var valueRect = new Rect(position.x, position.y + lineStep * 2f, position.width, lineHeight);
                EditorGUI.PropertyField(valueRect, valueProperty);
                GUI.enabled = previousEnabled;

                EditorGUI.indentLevel--;

                valueObject.ApplyModifiedProperties();
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            int lines = property.objectReferenceValue is HealthValue ? 3 : 1;
            return lines * EditorGUIUtility.singleLineHeight + (lines - 1) * EditorGUIUtility.standardVerticalSpacing;
        }

        private SerializedObject GetValueObject(SerializedProperty property, HealthValue healthValue)
        {
            string key = property.propertyPath;
            if (!_valueObjects.TryGetValue(key, out var valueObject) || valueObject.targetObject != healthValue)
            {
                valueObject = new SerializedObject(healthValue);
                _valueObjects[key] = valueObject;
            }

            return valueObject;
        }
    }
}

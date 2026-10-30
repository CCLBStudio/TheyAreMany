using CCLBStudio.EditorTools;
using CCLBStudio.ScriptableValue;
using UnityEditor;
using UnityEngine;

namespace Game.Utils
{
    [CustomEditor(typeof(HealthValue))]
    public class HealthValueEditor : Editor
    {
        private const float FieldLabelWidth = 70f;

        private SerializedProperty _valueProperty;
        private SerializedProperty _maxValueProperty;
        private SerializedProperty _initialValueProperty;
        private SerializedProperty _resetValueOnPlayModeExitProperty;
        private string[] _toExclude;

        private void OnEnable()
        {
            _valueProperty = serializedObject.FindProperty(ScriptableValue<float>.ValueProperty);
            _maxValueProperty = serializedObject.FindProperty(HealthValue.MaxValueProperty);
            _initialValueProperty = serializedObject.FindProperty(ScriptableValue<float>.InitialValueProperty);
            _resetValueOnPlayModeExitProperty = serializedObject.FindProperty(ScriptableValue<float>.ResetValueOnPlayModeExitProperty);
            _toExclude = new[]
            {
                "m_Script",
                ScriptableValue<float>.ValueProperty,
                HealthValue.MaxValueProperty,
                ScriptableValue<float>.InitialValueProperty,
                ScriptableValue<float>.ResetValueOnPlayModeExitProperty
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorExtender.DrawScriptField(serializedObject);

            bool valueLocked = Application.isPlaying && _resetValueOnPlayModeExitProperty.boolValue;
            if (valueLocked)
            {
                EditorGUILayout.HelpBox("Editing the Value property is disabled during play mode, because it will be reset to the Initial Value property when stop playing.", MessageType.Info);
            }

            float previousLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = FieldLabelWidth;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_maxValueProperty);
            GUI.enabled = !valueLocked;
            EditorGUILayout.PropertyField(_valueProperty);
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = previousLabelWidth;

            EditorGUILayout.PropertyField(_resetValueOnPlayModeExitProperty);

            GUI.enabled = false;
            EditorGUILayout.PropertyField(_initialValueProperty);
            GUI.enabled = true;

            DrawPropertiesExcluding(serializedObject, _toExclude);

            serializedObject.ApplyModifiedProperties();
        }
    }
}

using UnityEditor;
using UnityEngine;

namespace CCLBStudio.ScriptableValue
{
    [CustomEditor(typeof(ScriptableValue<>), true)]
    public class ScriptableValueEditor : Editor
    {
        private SerializedProperty _valueProperty;
        private SerializedProperty _initialValueProperty;
        
        private void OnEnable()
        {
            _valueProperty = serializedObject.FindProperty(ScriptableValue<dynamic>.ValueProperty);
            _initialValueProperty = serializedObject.FindProperty(ScriptableValue<dynamic>.InitialValueProperty);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Editing the Value property is disabled during play mode, because it will be reset to the Initial Value property when stop playing.", MessageType.Info);
                GUI.enabled = false;
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_valueProperty);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }

            GUI.enabled = false;
            EditorGUILayout.PropertyField(_initialValueProperty);
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
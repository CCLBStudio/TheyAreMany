using UnityEditor;
using UnityEngine;

namespace CCLBStudio.ScriptableValue
{
    [CustomEditor(typeof(BaseScriptableValue), true)]
    public class ScriptableValueEditor : Editor
    {
        private SerializedProperty _valueProperty;
        private SerializedProperty _initialValueProperty;
        private SerializedProperty _resetValueOnPlayModeExitProperty;
        
        private void OnEnable()
        {
            _valueProperty = serializedObject.FindProperty(ScriptableValue<dynamic>.ValueProperty);
            _initialValueProperty = serializedObject.FindProperty(ScriptableValue<dynamic>.InitialValueProperty);
            _resetValueOnPlayModeExitProperty = serializedObject.FindProperty(ScriptableValue<dynamic>.ResetValueOnPlayModeExitProperty);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            if (Application.isPlaying && _resetValueOnPlayModeExitProperty.boolValue)
            {
                EditorGUILayout.HelpBox("Editing the Value property is disabled during play mode, because it will be reset to the Initial Value property when stop playing.", MessageType.Info);
                GUI.enabled = false;
            }
            
            EditorGUI.BeginChangeCheck();
            //EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(_valueProperty);
            
            GUI.enabled = true;
            EditorGUILayout.PropertyField(_resetValueOnPlayModeExitProperty);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
            //EditorGUILayout.EndHorizontal();
            
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_initialValueProperty);
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
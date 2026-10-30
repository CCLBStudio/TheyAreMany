using System.Collections.Generic;
using CCLBStudio.EditorTools;
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
        private string[] _toExclude;
        
        private void OnEnable()
        {
            _valueProperty = serializedObject.FindProperty(ScriptableValue<dynamic>.ValueProperty);
            _initialValueProperty = serializedObject.FindProperty(ScriptableValue<dynamic>.InitialValueProperty);
            _resetValueOnPlayModeExitProperty = serializedObject.FindProperty(ScriptableValue<dynamic>.ResetValueOnPlayModeExitProperty);
            _toExclude = new string[] { "m_Script", ScriptableValue<dynamic>.ValueProperty, ScriptableValue<dynamic>.InitialValueProperty, ScriptableValue<dynamic>.ResetValueOnPlayModeExitProperty };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorExtender.DrawScriptField(serializedObject);
            
            if (Application.isPlaying && _resetValueOnPlayModeExitProperty.boolValue)
            {
                EditorGUILayout.HelpBox("Editing the Value property is disabled during play mode, because it will be reset to the Initial Value property when stop playing.", MessageType.Info);
                GUI.enabled = false;
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_valueProperty);
            
            GUI.enabled = true;
            EditorGUILayout.PropertyField(_resetValueOnPlayModeExitProperty);
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
            
            GUI.enabled = false;
            EditorGUILayout.PropertyField(_initialValueProperty);
            GUI.enabled = true;
            
            DrawPropertiesExcluding(serializedObject, _toExclude);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
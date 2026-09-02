using System.Collections.Generic;
using System.Linq;
using ReaaliStudio.Utils.Extensions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScriptableValueDatabase))]
public class ScriptableValueDatabaseEditor : Editor
{
    private List<SerializedProperty> _templateProperties;
    private List<SerializedProperty> _remainingProperties;

    private bool _isEditingTemplate;

    private void OnEnable()
    {
        _templateProperties = EditorExtender.GetAllPropertiesOfType(serializedObject, typeof(ScriptableValueDatabase.TemplateFile));
        _remainingProperties = EditorExtender.GetSelfPropertiesExcluding(serializedObject, _templateProperties.Select(x => x.propertyPath).ToArray());
    }

    private void OnDisable()
    {
        if (_isEditingTemplate)
        {
            _isEditingTemplate = false;
            RefreshIndexes();
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        EditorGUI.BeginChangeCheck();
        EditorExtender.DrawProperties(_templateProperties);
        if (EditorGUI.EndChangeCheck())
        {
            _isEditingTemplate = true;
        }

        if (_isEditingTemplate && !EditorGUIUtility.editingTextField)
        {
            _isEditingTemplate = false;
            RefreshIndexes();
        }
        
        EditorExtender.DrawProperties(_remainingProperties);
        serializedObject.ApplyModifiedProperties();
    }

    private void RefreshIndexes()
    {
        Debug.Log("Refresh all !");
        var script = (ScriptableValueDatabase)target;
        script.RefreshAllTemplateIndexes();
    }
}
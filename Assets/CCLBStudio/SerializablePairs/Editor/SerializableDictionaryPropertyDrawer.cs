using CCLBStudio.SerializablePairs;
using UnityEditor;
using UnityEngine;

//[CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
public class SerializableDictionaryPropertyDrawer : PropertyDrawer
{
    private bool labelUnfolded = true;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUILayout.Space(-20);
        
        
        var myStyle = EditorStyles.foldoutHeader;
        myStyle.fontStyle = FontStyle.Normal;
        labelUnfolded = EditorGUILayout.BeginFoldoutHeaderGroup(labelUnfolded, label, myStyle);
        EditorGUILayout.EndFoldoutHeaderGroup();

        if (!labelUnfolded)
        {
            return;
        }
        
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(property.FindPropertyRelative(SerializableDictionary<dynamic, dynamic>.InitialPairsPropertyName));
        GUI.enabled = false;
        EditorGUILayout.PropertyField(property.FindPropertyRelative(SerializableDictionary<dynamic, dynamic>.DebugDisplayPropertyName));
        GUI.enabled = true;
    
        EditorGUI.indentLevel--;

        property.serializedObject.ApplyModifiedProperties();
    }
}
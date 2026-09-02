using UnityEditor;
using UnityEngine;

namespace CCLBStudio.Attributes
{
    [CustomPropertyDrawer(typeof(NullInfoAttribute))]
    public class NullInfoDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            NullInfoAttribute nullInfoAttribute = (NullInfoAttribute)attribute;
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.PropertyField(position, property, label);

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.EndProperty();
                return;
            }
            
            if (!property.objectReferenceValue)
            {
                GUIStyle placeholderStyle = new GUIStyle(GUI.skin.label);
                placeholderStyle.normal.textColor = nullInfoAttribute.TextColor;
                    
                string pptrType = property.type;
                pptrType = pptrType.Remove(0, 6);
                pptrType = pptrType.Remove(pptrType.Length - 1, 1);
                string prefix = IsMissing(property) ? "Missing" : "None";
                Vector2 size = placeholderStyle.CalcSize(new GUIContent($"{prefix} ({ObjectNames.NicifyVariableName(pptrType)}) "));
                float posOffset = EditorGUIUtility.labelWidth + size.x - EditorGUI.indentLevel * 15;
                position.width = position.width - posOffset - 17;
                position.x += posOffset;
                EditorGUI.LabelField(position, new GUIContent(nullInfoAttribute.InfoText, label.tooltip), placeholderStyle);
            }

            EditorGUI.EndProperty();
        }
        
        private bool IsMissing(SerializedProperty p)
        {
            return p.objectReferenceEntityIdValue.IsValid();
        }
    }
}
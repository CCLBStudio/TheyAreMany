using UnityEditor;
using UnityEngine;

namespace CCLBStudio.StringIds
{
    [CustomPropertyDrawer(typeof(StringId), true)]
    public class StringIdDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var idType = StringIdEditorGUI.ResolveIdType(property);
            return idType == null ? EditorGUIUtility.singleLineHeight : StringIdEditorGUI.GetIdHeight(property, idType, required: false);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var idType = StringIdEditorGUI.ResolveIdType(property);
            if (idType == null)
            {
                EditorGUI.LabelField(position, label, new GUIContent("Unresolved StringId type"));
                return;
            }

            StringIdEditorGUI.DrawId(position, property, label, idType, required: false);
        }
    }
}

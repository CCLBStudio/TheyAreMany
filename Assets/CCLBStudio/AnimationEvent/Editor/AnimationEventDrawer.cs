using UnityEditor;
using UnityEngine;

namespace CCLBStudio.AnimationEvent
{
    [CustomPropertyDrawer(typeof(AnimationEvent))]
    public class AnimationEventDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            SerializedProperty eventName = property.FindPropertyRelative("eventName");
            SerializedProperty animationEvents = property.FindPropertyRelative("unityEvent");
            
            Rect stateNameRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            Rect eventRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUI.GetPropertyHeight(animationEvents));
            
            EditorGUI.PropertyField(stateNameRect, eventName);
            EditorGUI.PropertyField(eventRect, animationEvents, true);
            
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty animationEvents = property.FindPropertyRelative("unityEvent");
            return EditorGUIUtility.singleLineHeight + 4 + EditorGUI.GetPropertyHeight(animationEvents);
        }
    }
}

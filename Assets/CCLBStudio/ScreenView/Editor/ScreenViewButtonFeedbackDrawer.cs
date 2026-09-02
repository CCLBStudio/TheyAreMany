using System;
using CCLBStudio.EditorTools;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CCLBStudio.ScreenView
{
    [CustomPropertyDrawer(typeof(ScreenViewButtonFeedback))]
    public class ScreenViewButtonFeedbackDrawer : PropertyDrawer
    {
        private string _initFor;
        private SerializedProperty _target, _feedbacks;
        private ReorderableList _list;

        private void Initialize(SerializedProperty property)
        {
            _initFor = property.propertyPath;
            _target = property.FindPropertyRelative(ScreenViewButtonFeedback.TargetProperty);
            _feedbacks = property.FindPropertyRelative(ScreenViewButtonFeedback.FeedbacksProperty);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (string.IsNullOrEmpty(_initFor) || _initFor != property.propertyPath)
            {
                Initialize(property);
            }
            
            EnsureList(property);
            
            float targetHeight = EditorGUI.GetPropertyHeight(_target);
            var targetRect = new Rect(position.x, position.y, position.width, targetHeight);
            var listRect = new Rect(position.x, position.y + targetHeight + 2, position.width, _list.GetHeight());

            EditorGUI.PropertyField(targetRect, _target);
            _list.DoList(listRect);
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            EnsureList(property);
            return EditorGUI.GetPropertyHeight(_target) + _list.GetHeight() + 8;
        }
        
        private void EnsureList(SerializedProperty property)
        {
            if (_list != null) return;

            _feedbacks = property.FindPropertyRelative(ScreenViewButtonFeedback.FeedbacksProperty);
            _target = property.FindPropertyRelative(ScreenViewButtonFeedback.TargetProperty);

            _list = new ReorderableList(property.serializedObject, _feedbacks, true, true, true, true);

            _list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Feedbacks");
            };

            _list.drawElementCallback = (rect, index, active, focused) =>
            {
                rect.x += 10;
                rect.width -= 10;
                var element = _feedbacks.GetArrayElementAtIndex(index);
                EditorGUI.PropertyField(rect, element, new GUIContent($"Element {index}"), true);
            };

            _list.elementHeightCallback = index =>
            {
                var element = _feedbacks.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element) + 4;
            };

            _list.onAddDropdownCallback = (buttonRect, list) =>
            {
                var menu = new GenericMenu();

                var feedbacksTypes = EditorExtender.GetAllConcreteTypes(typeof(IScreenViewFeedback));
                foreach (var type in feedbacksTypes)
                {
                    menu.AddItem(new GUIContent(ObjectNames.NicifyVariableName(type.Name)), false, () =>
                    {
                        var instance = Activator.CreateInstance(type);
                        _feedbacks.arraySize++;
                        var newElement = _feedbacks.GetArrayElementAtIndex(_feedbacks.arraySize - 1);
                        newElement.managedReferenceValue = instance;
                        _feedbacks.serializedObject.ApplyModifiedProperties();
                        
                        var updatedElement = _feedbacks.GetArrayElementAtIndex(_feedbacks.arraySize - 1);
                        var managedObj = updatedElement.managedReferenceValue as IScreenViewFeedback;
                        var view = _feedbacks.serializedObject.targetObject as ScreenView;
                        if (!view)
                        {
                            Debug.LogError("Target object is not a screen view !");
                        }
                        else
                        {
                            managedObj?.OnEditorCreated(view);
                        }
                    });
                }

                if (feedbacksTypes.Count == 0)
                {
                    menu.AddDisabledItem(new GUIContent("No valid IScreenViewFeedback types found"));
                }

                menu.ShowAsContext();
            };
        }
    }
}

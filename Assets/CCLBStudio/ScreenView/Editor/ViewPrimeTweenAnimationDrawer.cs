using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CCLBStudio.ScreenView
{
    [CustomPropertyDrawer(typeof(ViewPrimeTweenAnimation))]
    public class ViewPrimeTweenAnimationDrawer : PropertyDrawer
    {
public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Vector2 pos = position.min;
            Vector2 size = new Vector2(position.size.x, EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing);
            Rect customRect = new Rect(pos, size);
            property.isExpanded = EditorGUI.Foldout(customRect, property.isExpanded, label, true);
            if (!property.isExpanded)
            {
                return;
            }

            var type = property.FindPropertyRelative(ScreenViewTweenDescriptor.TypeProperty);
            var initialDelay = property.FindPropertyRelative(ScreenViewTweenDescriptor.InitialDelayProperty);

            EditorGUI.indentLevel++;

            DrawProperty(ref customRect, type);
            DrawProperty(ref customRect, initialDelay);
        
            ScreenViewTweenType tweenType = (ScreenViewTweenType)type.enumValueIndex;
            switch (tweenType)
            {
                case ScreenViewTweenType.CanvasGroup:
                    var canvasGroup = property.FindPropertyRelative(ScreenViewTweenDescriptor.CanvasGroupProperty);
                    DrawProperty(ref customRect, canvasGroup);
                    if (!canvasGroup.objectReferenceValue)
                    {
                        Rect helpBoxRect = new Rect(customRect);
                        helpBoxRect.y += helpBoxRect.height + EditorGUIUtility.standardVerticalSpacing * 2;
                        helpBoxRect.height *= 2;
                        EditorGUI.HelpBox(helpBoxRect, "You need to assign a valid Canvas Group.", MessageType.Error);
                        break;
                    }
                    DrawProperty(ref customRect, property.FindPropertyRelative(ScreenViewTweenDescriptor.FadeFromProperty));
                    DrawProperty(ref customRect, property.FindPropertyRelative(ScreenViewTweenDescriptor.FadeToProperty));
                
                    var fadeDuration = property.FindPropertyRelative(ScreenViewTweenDescriptor.FadeDurationProperty);
                    var customFadeCurve = property.FindPropertyRelative(ScreenViewTweenDescriptor.CustomFadeCurveProperty);
                    var fadeCurve = property.FindPropertyRelative(ScreenViewTweenDescriptor.FadeCurveProperty);
                    var fadeEase = property.FindPropertyRelative(ScreenViewTweenDescriptor.FadeEaseProperty);

                    DrawProperty(ref customRect, fadeDuration);
                    DrawProperty(ref customRect, customFadeCurve);
                    DrawProperty(ref customRect, customFadeCurve.boolValue ? fadeCurve : fadeEase);
                    break;
            
                case ScreenViewTweenType.RectTransform:
                    var rect = property.FindPropertyRelative(ScreenViewTweenDescriptor.RectProperty);
                    DrawProperty(ref customRect, rect);
                    if (!rect.objectReferenceValue)
                    {
                        Rect helpBoxRect = new Rect(customRect);
                        helpBoxRect.y += helpBoxRect.height + EditorGUIUtility.standardVerticalSpacing * 2;
                        helpBoxRect.height *= 2;
                        EditorGUI.HelpBox(helpBoxRect, "You need to assign a valid Rect Transform.", MessageType.Error);
                        break;
                    }
                    var move = property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveRectProperty);
                    DrawProperty(ref customRect, move);
                    if (move.boolValue)
                    {
                        DrawProperty(ref customRect, property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveDirectionProperty));
                        DrawProperty(ref customRect, property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveFromOffsetProperty));

                        var moveDuration = property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveDurationProperty);
                        var customMoveCurve = property.FindPropertyRelative(ScreenViewTweenDescriptor.CustomMoveCurveProperty);
                        var moveCurve = property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveCurveProperty);
                        var moveEase = property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveEaseProperty);
                    
                        DrawProperty(ref customRect, moveDuration);
                        DrawProperty(ref customRect, customMoveCurve);
                        DrawProperty(ref customRect, customMoveCurve.boolValue ? moveCurve : moveEase);
                    }

                    var scale = property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleRectProperty);
                    DrawProperty(ref customRect, scale);
                    if (scale.boolValue)
                    {
                        DrawProperty(ref customRect, property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleFromProperty));
                        DrawProperty(ref customRect, property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleToProperty));
                    
                        var scaleDuration = property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleDurationProperty);
                        var customScaleCurve = property.FindPropertyRelative(ScreenViewTweenDescriptor.CustomScaleCurveProperty);
                        var scaleCurve = property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleCurveProperty);
                        var scaleEase = property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleEaseProperty);
                    
                        DrawProperty(ref customRect, scaleDuration);
                        DrawProperty(ref customRect, customScaleCurve);
                        DrawProperty(ref customRect, customScaleCurve.boolValue ? scaleCurve : scaleEase);
                    }
                    break;
            
                default:
                    throw new ArgumentOutOfRangeException();
            }
        
            EditorGUI.indentLevel--;

            EditorGUI.EndProperty();
        }
    
        private void DrawProperty(ref Rect position, SerializedProperty subProperty)
        {
            position.y += EditorGUI.GetPropertyHeight(subProperty) + EditorGUIUtility.standardVerticalSpacing * 2;
            EditorGUI.PropertyField(position, subProperty);
        }

        private List<SerializedProperty> FetchRelevantProperties(SerializedProperty property)
        {
            List<SerializedProperty> result = new List<SerializedProperty>(20)
            {
                property.FindPropertyRelative(ScreenViewTweenDescriptor.TypeProperty),
                property.FindPropertyRelative(ScreenViewTweenDescriptor.InitialDelayProperty),
            };

            ScreenViewTweenType tweenType = (ScreenViewTweenType)property.FindPropertyRelative(ScreenViewTweenDescriptor.TypeProperty).enumValueIndex;
            switch (tweenType)
            {
                case ScreenViewTweenType.CanvasGroup:
                    result.Add(property.FindPropertyRelative(ScreenViewTweenDescriptor.CanvasGroupProperty));
                    result.Add(property.FindPropertyRelative(ScreenViewTweenDescriptor.FadeFromProperty));
                    result.Add(property.FindPropertyRelative(ScreenViewTweenDescriptor.FadeToProperty));
                    result.Add(property.FindPropertyRelative(ScreenViewTweenDescriptor.FadeDurationProperty));
                
                    var customFadeCurve = property.FindPropertyRelative(ScreenViewTweenDescriptor.CustomFadeCurveProperty);
                    result.Add(customFadeCurve);
                    result.Add(customFadeCurve.boolValue ? property.FindPropertyRelative(ScreenViewTweenDescriptor.FadeCurveProperty) : property.FindPropertyRelative(ScreenViewTweenDescriptor.FadeEaseProperty));
                    break;
            
                case ScreenViewTweenType.RectTransform:
                    var move = property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveRectProperty);
                    var scale = property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleRectProperty);
                
                    result.Add(property.FindPropertyRelative(ScreenViewTweenDescriptor.RectProperty));
                    result.Add(move);
                    result.Add(scale);

                    if (move.boolValue)
                    {
                        result.Add(property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveDirectionProperty));
                        result.Add(property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveDurationProperty));
                        result.Add(property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveFromOffsetProperty));
                    
                        var customMoveCurve = property.FindPropertyRelative(ScreenViewTweenDescriptor.CustomMoveCurveProperty);
                        result.Add(customMoveCurve);
                        result.Add(customMoveCurve.boolValue ? property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveCurveProperty) : property.FindPropertyRelative(ScreenViewTweenDescriptor.MoveEaseProperty));
                    }

                    if (scale.boolValue)
                    {
                        result.Add(property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleFromProperty));
                        result.Add(property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleToProperty));
                        result.Add(property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleDurationProperty));
                    
                        var customScaleCurve = property.FindPropertyRelative(ScreenViewTweenDescriptor.CustomScaleCurveProperty);
                        result.Add(customScaleCurve);
                        result.Add(customScaleCurve.boolValue ? property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleCurveProperty) : property.FindPropertyRelative(ScreenViewTweenDescriptor.ScaleEaseProperty));
                    }
                    break;
            
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float totalHeight = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;

            if (!property.isExpanded)
            {
                return totalHeight;
            }
        
            foreach (var p in FetchRelevantProperties(property))
            {
                totalHeight += EditorGUI.GetPropertyHeight(p) + EditorGUIUtility.standardVerticalSpacing * 2;
            }

            return totalHeight;
        }
    
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;

namespace CCLBStudio.ScreenView
{
    [Serializable]
    public class ScreenViewTweenDescriptor
    {
        #region Editor

        #if UNITY_EDITOR

        public static string TypeProperty => nameof(type);
        public static string InitialDelayProperty => nameof(initialDelay);
        public static string CanvasGroupProperty => nameof(canvasGroup);
        public static string FadeFromProperty => nameof(fadeFrom);
        public static string FadeToProperty => nameof(fadeTo);
        public static string FadeDurationProperty => nameof(fadeDuration);
        public static string CustomFadeCurveProperty => nameof(customFadeCurve);
        public static string FadeCurveProperty => nameof(fadeCurve);
        public static string FadeEaseProperty => nameof(fadeEase);
        public static string RectProperty => nameof(rect);
        public static string MoveRectProperty => nameof(moveRect);
        public static string MoveDirectionProperty => nameof(moveDirection);
        public static string MoveFromOffsetProperty => nameof(normalizedScreenOffset);
        public static string MoveDurationProperty => nameof(moveDuration);
        public static string CustomMoveCurveProperty => nameof(customMoveCurve);
        public static string MoveCurveProperty => nameof(moveCurve);
        public static string MoveEaseProperty => nameof(moveEase);
        public static string ScaleRectProperty => nameof(scaleRect);
        public static string ScaleFromProperty => nameof(scaleFrom);
        public static string ScaleToProperty => nameof(scaleTo);
        public static string ScaleDurationProperty => nameof(scaleDuration);
        public static string CustomScaleCurveProperty => nameof(customScaleCurve);
        public static string ScaleCurveProperty => nameof(scaleCurve);
        public static string ScaleEaseProperty => nameof(scaleEase);

#endif

        #endregion

        public event Action OnCompleted;
        
        // common fields
        public ScreenViewTweenType type;
        [Min(0f)] public float initialDelay;

        // fade fields
        public CanvasGroup canvasGroup;
        public float fadeFrom= 0f;
        public float fadeTo = 1f;
        public float fadeDuration = 1f;
        public bool customFadeCurve;
        public AnimationCurve fadeCurve;
        public Ease fadeEase = Ease.OutQuart;

        // rect movement fields
        public RectTransform rect;
        public bool moveRect;
        public ScreenViewTweenDirection moveDirection = ScreenViewTweenDirection.FromEast;
        public Vector2 normalizedScreenOffset;
        public float moveDuration = 1f;
        public bool customMoveCurve;
        public AnimationCurve moveCurve;
        public Ease moveEase = Ease.OutQuart;

        // rect scale fields
        public bool scaleRect;
        public float scaleFrom;
        public float scaleTo;
        public float scaleDuration = 1f;
        public bool customScaleCurve;
        public AnimationCurve scaleCurve;
        public Ease scaleEase = Ease.OutQuart;

        private Canvas _canvas;
        private Vector2? _referenceAnchoredPos;

        public List<Tween> Launch()
        {
            if (!_canvas && rect)
            {
                _canvas = rect.GetComponentInParent<Canvas>();
            }

            return type switch
            {
                ScreenViewTweenType.CanvasGroup => HandleCanvasGroupAnimation(),
                ScreenViewTweenType.RectTransform => HandleRectAnimation(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private List<Tween> HandleCanvasGroupAnimation()
        {
            List<Tween> launched = new List<Tween>();
            if (!canvasGroup)
            {
                return launched;
            }
            
            canvasGroup.alpha = fadeFrom;
            var tween = Tween.Alpha(canvasGroup, fadeTo, duration: fadeDuration, startDelay: initialDelay, ease: customFadeCurve ? fadeCurve : fadeEase);

            tween.OnComplete(LaunchOnCompleted);
            launched.Add(tween);
            return launched;
        }
        
        private List<Tween> HandleRectAnimation()
        {
            List<Tween> launched = new List<Tween>();
            if (!rect)
            {
                return launched;
            }
            
            if (moveRect)
            {
                PlaceRectAtStartPos(out Vector2 destination);
                var tween = Tween.UIAnchoredPosition(rect, destination, duration: moveDuration, startDelay: initialDelay, ease: customMoveCurve ? moveCurve : moveEase);
                
                launched.Add(tween);
            }

            if (scaleRect)
            {
                rect.localScale = Vector3.one * scaleFrom;
                var tween = Tween.Scale(rect, Vector3.one * scaleTo, duration: scaleDuration, startDelay: initialDelay, ease: customScaleCurve ? scaleCurve : scaleEase);

                launched.Add(tween);
            }

            if (launched.Count > 0)
            {
                var longestTween = launched.Aggregate((max, current) => current.duration > max.duration ? current : max);
                longestTween.OnComplete(LaunchOnCompleted);
            }
            else
            {
                LaunchOnCompleted();
            }
            
            return launched;
        }

        private void LaunchOnCompleted()
        {
            OnCompleted?.Invoke();
        }

        private void PlaceRectAtStartPos(out Vector2 destination)
        {
            float scaleFactor = _canvas ? _canvas.scaleFactor : 1f;
            Vector2 offset = new Vector2(Screen.width / scaleFactor * Mathf.Clamp(normalizedScreenOffset.x, -1f, 1f), Screen.height / scaleFactor * Mathf.Clamp(normalizedScreenOffset.y, -1f, 1f));
            _referenceAnchoredPos ??= rect.anchoredPosition;
            Vector2 startPos = _referenceAnchoredPos.Value;
            destination = startPos;
            
            switch (moveDirection)
            {
                case ScreenViewTweenDirection.FromSouth:
                    startPos = new Vector2(0f, -Screen.height / scaleFactor) + offset;
                    break;
                    
                case ScreenViewTweenDirection.FromNorth:
                    startPos = new Vector2(0f, Screen.height / scaleFactor) + offset;
                    break;
                    
                case ScreenViewTweenDirection.FromEast:
                    startPos = new Vector2(Screen.width / scaleFactor, 0f) + offset;
                    break;
                    
                case ScreenViewTweenDirection.FromWest:
                    startPos = new Vector2(-Screen.width / scaleFactor, 0f) + offset;
                    break;

                case ScreenViewTweenDirection.ToSouth:
                    destination = new Vector2(0f, -Screen.height / scaleFactor) + offset;
                    break;
                    
                case ScreenViewTweenDirection.ToNorth:
                    destination = new Vector2(0f, Screen.height / scaleFactor) + offset;
                    break;

                case ScreenViewTweenDirection.ToEast:
                    destination = new Vector2(Screen.width / scaleFactor, 0f) + offset;
                    break;

                case ScreenViewTweenDirection.ToWest:
                    destination = new Vector2(-Screen.width / scaleFactor, 0f) + offset;
                    break;
            }

            rect.anchoredPosition = startPos;
        }

        public float GetTotalTime()
        {
            float result = initialDelay;

            switch (type)
            {
                case ScreenViewTweenType.CanvasGroup:
                    result += fadeDuration;
                    break;
                
                case ScreenViewTweenType.RectTransform:
                    if (moveRect)
                    {
                        if (scaleRect)
                        {
                            result += Mathf.Max(moveDuration, scaleDuration);
                        }
                        else
                        {
                            result += moveDuration;
                        }
                    }
                    else if(scaleRect)
                    {
                        result += scaleDuration;
                    }
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return result;
        }
    }
    
    public enum ScreenViewTweenType {CanvasGroup, RectTransform}
    public enum ScreenViewTweenDirection {FromSouth, FromNorth, FromEast, FromWest, ToSouth, ToNorth, ToEast, ToWest}
}
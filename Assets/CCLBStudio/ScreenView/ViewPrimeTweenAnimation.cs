using System;
using System.Collections.Generic;
using System.Linq;
using PrimeTween;
using UnityEngine;

namespace CCLBStudio.ScreenView
{
    [Serializable]
    public class ViewPrimeTweenAnimation : IScreenViewAnimation
    {
        // common fields
        public ScreenViewTweenType type;
        [Min(0f)] public float initialDelay;

        // fade fields
        public CanvasGroup canvasGroup;
        public float fadeFrom= 0f;
        public float fadeTo = 1f;
        public float fadeDuration = .5f;
        public bool customFadeCurve;
        public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public Ease fadeEase = Ease.OutQuart;

        // rect movement fields
        public RectTransform rect;
        public bool moveRect;
        public ScreenViewTweenDirection moveDirection = ScreenViewTweenDirection.FromEast;
        public Vector2 normalizedScreenOffset;
        public float moveDuration = .5f;
        public bool customMoveCurve;
        public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public Ease moveEase = Ease.OutQuart;

        // rect scale fields
        public bool scaleRect;
        public float scaleFrom;
        public float scaleTo;
        public float scaleDuration = .5f;
        public bool customScaleCurve;
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        public Ease scaleEase = Ease.OutQuart;

        private Canvas _canvas;
        private Vector2? _referenceAnchoredPos;
        private List<Tween> _currentlyRunning = new();

        public event Action OnCompleted;

        public void Animate()
        {
            if (!_canvas && rect)
            {
                _canvas = rect.GetComponentInParent<Canvas>();
            }

            switch (type)
            {
                case ScreenViewTweenType.CanvasGroup:
                    HandleCanvasGroupAnimation();
                    break;
                
                case ScreenViewTweenType.RectTransform:
                    HandleRectAnimation();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Kill()
        {
            foreach (var t in _currentlyRunning.Where(x => x.isAlive))
            {
                t.Stop();
            }
        }

        public void OnEditorCreated(ScreenView view)
        {
            canvasGroup = view.GetComponent<CanvasGroup>();
            rect = (RectTransform)view.transform;
        }

        private void HandleCanvasGroupAnimation()
        {
            _currentlyRunning = new List<Tween>();
            if (!canvasGroup)
            {
                return;
            }
            
            canvasGroup.alpha = fadeFrom;
            var tween = Tween.Alpha(canvasGroup, endValue: fadeTo, duration: fadeDuration, startDelay: initialDelay, ease:customFadeCurve ? fadeCurve : fadeEase);

            tween.OnComplete(LaunchOnCompleted);
            _currentlyRunning.Add(tween);
        }
        
        private void HandleRectAnimation()
        {
            _currentlyRunning = new List<Tween>();
            if (!rect)
            {
                return;
            }
            
            if (moveRect)
            {
                PlaceRectAtStartPos(out Vector2 destination);
                var tween = Tween.UIAnchoredPosition(rect, destination, duration: moveDuration, startDelay: initialDelay, ease: customMoveCurve ? moveCurve : moveEase);
                
                _currentlyRunning.Add(tween);
            }

            if (scaleRect)
            {
                rect.localScale = Vector3.one * scaleFrom;
                var tween = Tween.Scale(rect, Vector3.one * scaleTo, duration: scaleDuration, startDelay: initialDelay, ease: customScaleCurve ? scaleCurve : scaleEase);
                
                _currentlyRunning.Add(tween);
            }

            if (_currentlyRunning.Count > 0)
            {
                var longestTween = _currentlyRunning.Aggregate((max, current) => current.duration > max.duration ? current : max);
                longestTween.OnComplete(LaunchOnCompleted);
            }
            else
            {
                LaunchOnCompleted();
            }
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
}
using System.Collections.Generic;
using CCLBStudio.DependencyInjection;
using Services;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Transition
{
    [CreateAssetMenu(fileName = "TransitionService", menuName = "They Are Many/Transition/Service")]
    [Provide]
    public partial class TransitionService : AppService
    {
        [SerializeField] private Dictionary<TransitionId, TransitionProfile> profiles = new();

        private TransitionAnimator _lastFadeInAnimator;
        private TransitionAnimator _lastFadeOutAnimator;
        private readonly Dictionary<TransitionId, AwaitableCompletionSource> _currentlyRunningTransitions = new();
        
        public async Awaitable FadeIn(TransitionId transitionId)
        {
            if (IsTransitioning(transitionId))
            {
                await _currentlyRunningTransitions[transitionId].Awaitable;
                return;
            }
            
            var acs = new AwaitableCompletionSource();
            _currentlyRunningTransitions.Add(transitionId, acs);
            var animator = GetTransitionAnimator(transitionId, false);
            AddEndHandler(transitionId, animator, acs);
            _lastFadeInAnimator = animator;
            await acs.Awaitable;
        }
        
        public async Awaitable FadeOut(TransitionId transitionId)
        {
            if (IsTransitioning(transitionId))
            {
                await _currentlyRunningTransitions[transitionId].Awaitable;
                return;
            }
            
            if (_lastFadeInAnimator)
            {
                _lastFadeInAnimator.onTransitionEnd?.Invoke();
                Destroy(_lastFadeInAnimator.gameObject);
                _lastFadeInAnimator = null;
            }
            
            var acs = new AwaitableCompletionSource();
            _currentlyRunningTransitions.Add(transitionId, acs);
            var animator = GetTransitionAnimator(transitionId, true);
            AddEndHandler(transitionId, animator, acs);
            await acs.Awaitable;
        }

        private bool IsTransitioning(TransitionId id)
        {
            return _currentlyRunningTransitions.ContainsKey(id);
        }
        
        
        private TransitionAnimator GetTransitionAnimator(TransitionId transitionId, bool autoDestroy)
        {
            
            return profiles.TryGetValue(transitionId, out var profile) 
                ? TransitionAnimator.Start(profile, autoDestroy: autoDestroy, destroyDelay: 0f)
                : TransitionAnimator.Start(TransitionType.Fade, duration: 1f, color: Color.black, autoDestroy: autoDestroy, destroyDelay: 0f);
        }

        private void AddEndHandler(TransitionId id, TransitionAnimator animator, AwaitableCompletionSource acs)
        {
            UnityAction onEndHandler = null;
            onEndHandler = () =>
            {
                animator.onTransitionEnd.RemoveListener(onEndHandler);
                _currentlyRunningTransitions.Remove(id);
                acs.SetResult();
            };

            animator.onTransitionEnd.AddListener(onEndHandler);
        }
    }
}

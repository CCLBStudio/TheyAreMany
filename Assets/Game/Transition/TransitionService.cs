using System.Collections.Generic;
using Services;
using TransitionsPlus;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Transition
{
    [CreateAssetMenu(fileName = "TransitionService", menuName = "They Are Many/Transition/Service")]
    public class TransitionService : AppService
    {
        [SerializeField] private Dictionary<TransitionId, TransitionProfile> profiles = new();
        [SerializeField] private Dictionary<string, string> test = new();
        
        public async Awaitable FadeIn(TransitionId transitionId)
        {
            var acs = new AwaitableCompletionSource();
            var animator = GetTransitionAnimator(transitionId);
            AddEndHandler(animator, acs);
            await acs.Awaitable;
        }
        
        private TransitionAnimator GetTransitionAnimator(TransitionId transitionId)
        {
            return profiles.TryGetValue(transitionId, out var profile) 
                ? TransitionAnimator.Start(profile)
                : TransitionAnimator.Start(TransitionType.Fade, duration: 1f, color: Color.black);
        }

        private static void AddEndHandler(TransitionAnimator animator, AwaitableCompletionSource acs)
        {
            UnityAction onEndHandler = null;
            onEndHandler = () =>
            {
                animator.onTransitionEnd.RemoveListener(onEndHandler);
                acs.SetResult();
            };

            animator.onTransitionEnd.AddListener(onEndHandler);
        }
    }
}

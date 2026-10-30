using CCLBStudio.DependencyInjection;
using CCLBStudio.ScriptableBehaviours;
using UnityEngine;

namespace Game.Transition
{
    [CreateAssetMenu(fileName = "PlayTransitionAction", menuName = "They Are Many/Actions/PlayTransition")]
    public partial class PlayTransitionAction : ScriptableAction
    {
        [SerializeField] private TransitionId transitionId;
        [SerializeField] private TransitionType transitionType = TransitionType.FadeIn;
        
        [Inject] private TransitionService _transitionService;
        
        private enum TransitionType
        {
            FadeIn,
            FadeOut
        }

        public override void Execute()
        {
            switch (transitionType)
            {
                case TransitionType.FadeIn:
                    _transitionService.FadeIn(transitionId);
                    break;
                case TransitionType.FadeOut:
                    _transitionService.FadeOut(transitionId);
                    break;
            }
        }
    }
}

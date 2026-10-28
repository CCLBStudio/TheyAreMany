using System;
using CCLBStudio.StringIds;

namespace Game.Transition
{
    [Serializable]
    public sealed class TransitionId : StringId
    {
        public static readonly TransitionId Empty = new();
        public static readonly TransitionId FadeVignette = new("FadeVignette");
        
        public TransitionId() { }
        public TransitionId(string transitionId) : base(transitionId) { }
    }
}

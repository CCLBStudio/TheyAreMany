using System;
using CCLBStudio.StringIds;

namespace Game.Transition
{
    [Serializable]
    public sealed class TransitionId : StringId
    {
        public static readonly TransitionId Empty = new();
        public static readonly TransitionId VignetteFadeIn = new("VignetteFadeIn");
        public static readonly TransitionId VignetteFadeOut = new("VignetteFadeOut");
        
        public TransitionId() { }
        public TransitionId(string transitionId) : base(transitionId) { }
    }
}

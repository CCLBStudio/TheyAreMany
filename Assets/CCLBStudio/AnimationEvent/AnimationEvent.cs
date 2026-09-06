using System;
using UnityEngine.Events;

namespace CCLBStudio.AnimationEvent
{
    [Serializable]
    public class AnimationEvent
    {
        public string eventName;
        public UnityEvent unityEvent;
    }
}

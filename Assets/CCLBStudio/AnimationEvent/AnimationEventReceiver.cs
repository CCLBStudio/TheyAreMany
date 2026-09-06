using System.Linq;
using UnityEngine;

namespace CCLBStudio.AnimationEvent
{
    public class AnimationEventReceiver : MonoBehaviour
    {
        [SerializeField] private AnimationEvent[] animationEvents;

        public void TriggerAnimationEvent(string eventName)
        {
            var e = animationEvents.First(x => x.eventName == eventName);
            e.unityEvent?.Invoke();
        }
    }
}

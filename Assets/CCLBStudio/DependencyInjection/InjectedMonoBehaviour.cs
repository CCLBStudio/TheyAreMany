using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public abstract class InjectedMonoBehaviour : MonoBehaviour
    {
        [SerializeField] private MonoInjectEvent injectAt = MonoInjectEvent.Start;
        private enum MonoInjectEvent
        {
            Awake,
            OnEnable,
            Start
        }
        
        protected virtual void Awake()
        {
            if (injectAt == MonoInjectEvent.Awake)
            {
                TryInject();
            }
        }
        
        protected virtual void OnEnable()
        {
            if (injectAt == MonoInjectEvent.OnEnable)
            {
                TryInject();
            }
        }
        
        protected virtual void Start()
        {
            if (injectAt == MonoInjectEvent.Start)
            {
                TryInject();
            }
        }

        private void TryInject()
        {
            if (this is IGeneratedInjectable injectable)
            {
                injectable.InjectDependencies(); 
            }
        }
    }
}

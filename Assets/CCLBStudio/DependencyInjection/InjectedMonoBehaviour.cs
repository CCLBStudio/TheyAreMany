using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public abstract class InjectedMonoBehaviour : MonoBehaviour
    {
        [SerializeField] private InjectAt injectAt = InjectAt.Start;
        private enum InjectAt
        {
            Awake,
            OnEnable,
            Start
        }
        
        protected virtual void Awake()
        {
            if (injectAt == InjectAt.Awake)
            {
                TryInject();
            }
        }
        
        protected virtual void OnEnable()
        {
            if (injectAt == InjectAt.OnEnable)
            {
                TryInject();
            }
        }
        
        protected virtual void Start()
        {
            if (injectAt == InjectAt.Start)
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

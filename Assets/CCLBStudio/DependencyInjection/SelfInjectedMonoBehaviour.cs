using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public class SelfInjectedMonoBehaviour : MonoBehaviour
    {
        protected virtual void Start()
        {
            Injector.InjectNewConsumer(this);
        }
    }
}
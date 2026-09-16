using System.Collections.Generic;
using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    [DefaultExecutionOrder(-10)]
    public class SceneInjector : MonoBehaviour
    {
        public List<MonoBehaviour> bakedInjectables = new();

        private void Awake()
        {
            for (int i = 0; i < bakedInjectables.Count; i++)
            {
                var component = bakedInjectables[i];
            
                if (component != null && component is IGeneratedInjectable injectable)
                {
                    injectable.InjectDependencies();
                }
            }
        }
    }
}
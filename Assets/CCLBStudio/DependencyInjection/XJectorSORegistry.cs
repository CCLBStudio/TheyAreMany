using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    [CreateAssetMenu(fileName = "XJectorSORegistry", menuName = "CCLB Studio/XJector/XJectorSORegistry")]
    public class XJectorSORegistry : ScriptableObject
    {
        public ScriptableObject[] toProvide;
    }

    public static class XJectorSORegistryLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadRegistry()
        {
            var registry = Resources.Load<XJectorSORegistry>("XJectorSORegistry");
            if (!registry) return;
            
            foreach (var so in registry.toProvide)
            {
                if (!so)
                {
                    continue;
                }
                
                XJectorContainer.Provide(so.GetType(), so);
            }
        }
    }
}

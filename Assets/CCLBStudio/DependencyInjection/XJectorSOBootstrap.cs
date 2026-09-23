using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    [CreateAssetMenu(fileName = "XJectorSOBootstrap", menuName = "CCLB Studio/XJector/XJectorSOBootstrap")]
    public class XJectorSOBootstrap : ScriptableObject
    {
        public static string Name => "XJectorSOBootstrap";
        public ScriptableObject[] toProvide;
        public ScriptableObject[] toInject;
    }

    public static class XJectorSORegistryLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void LoadRegistry()
        {
            var registry = Resources.Load<XJectorSOBootstrap>(XJectorSOBootstrap.Name);
            if (!registry) return;
            
            foreach (var so in registry.toProvide)
            {
                if (!so)
                {
                    continue;
                }
                
                XJectorContainer.Provide(so.GetType(), so);
            }

            foreach (var so in registry.toInject)
            {
                if (!so || so is not IGeneratedInjectable i)
                {
                    continue;
                }

                i.InjectDependencies();
            }
        }
    }
}

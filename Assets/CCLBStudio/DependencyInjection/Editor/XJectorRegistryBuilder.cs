using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace CCLBStudio.DependencyInjection
{
    public class XJectorRegistryBuilder : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            UpdateRegistry();
        }

        [InitializeOnEnterPlayMode]
        public static void UpdateRegistry()
        {
            string folderPath = "Assets/CCLBStudio/DependencyInjection/Resources";
            string registryPath = $"{folderPath}/XJectorSORegistry.asset";
        
            var registry = AssetDatabase.LoadAssetAtPath<XJectorSORegistry>(registryPath);
            if (registry == null)
            {
                if (!AssetDatabase.IsValidFolder(folderPath))
                    AssetDatabase.CreateFolder("Assets/CCLBStudio/DependencyInjection", "Resources");
                
                registry = ScriptableObject.CreateInstance<XJectorSORegistry>();
                AssetDatabase.CreateAsset(registry, registryPath);
            }

            var typesWithProvide = TypeCache.GetTypesWithAttribute<ProvideAttribute>()
                .Where(t => t.IsSubclassOf(typeof(ScriptableObject)));

            var instancesToInject = new List<ScriptableObject>();

            foreach (var type in typesWithProvide)
            {
                string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    var asset = AssetDatabase.LoadAssetAtPath(path, type) as ScriptableObject;
                    if (asset)
                    {
                        instancesToInject.Add(asset);
                    }
                }
            }

            registry.toProvide = instancesToInject.ToArray();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
        }
    }
}
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CCLBStudio.DependencyInjection
{
    [InitializeOnLoad]
    public class SceneInjectorBaker : IProcessSceneWithReport
    {
        public int callbackOrder => 0; 

        static SceneInjectorBaker()
        {
            EditorSceneManager.sceneSaving += OnSceneSaving;
        }

        private static void OnSceneSaving(Scene scene, string path)
        {
            BakeScene(scene);
        }

        public void OnProcessScene(Scene scene, BuildReport report)
        {
            BakeScene(scene);
        }

        private static void BakeScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded) return;
            
            var allMonoBehaviours = Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
        
            var injectables = allMonoBehaviours
                .Where(m => m is IGeneratedInjectable)
                .ToList();

            var sceneInjector = Object.FindAnyObjectByType<SceneInjector>(FindObjectsInactive.Include);
        
            if (injectables.Count == 0)
            {
                if (sceneInjector)
                {
                    Debug.Log($"[XJector] No injectables found in scene {scene.name}. Removing {nameof(SceneInjector)}.");
                    Object.DestroyImmediate(sceneInjector.gameObject);
                }
                return;
            }

            if (!sceneInjector)
            {
                var go = new GameObject("[DI_SceneInjector]");
                sceneInjector = go.AddComponent<SceneInjector>();
                // go.hideFlags = HideFlags.HideInHierarchy; 
            }

            Debug.Log($"[XJector] Baking {scene.name}");
            sceneInjector.bakedInjectables = injectables;
            EditorUtility.SetDirty(sceneInjector);
        }
    }
}
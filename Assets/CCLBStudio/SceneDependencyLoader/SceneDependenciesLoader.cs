using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Tools
{
    /// <summary>
    /// SceneDependenciesLoader is responsible for predefining a list of scenes
    /// required to be loaded as dependencies for the current Unity scene. Scenes
    /// listed in the requiredScenes property can be used to streamline workflows
    /// that involve multiple scenes.
    /// </summary>
    /// <remarks>
    /// This script is for editor only.
    /// Any script depending on objects from required scenes should NOT access them in Awake()
    /// and should defer such access to OnEnable() or Start().
    /// Otherwise, some null refs could happen (especially from the dependency injector pattern)
    /// </remarks>
    [DefaultExecutionOrder(-100)]
    public class SceneDependenciesLoader : MonoBehaviour
    {
        #if UNITY_EDITOR
        public List<SceneAsset> requiredScenes;
        #endif

        /// <summary>
        /// This method is primarily used to ensure that all required scenes specified in the <c>requiredScenes</c>
        /// list are loaded as dependencies during play mode in the Unity Editor. If the
        /// application is running outside the Unity Editor, the component is destroyed.
        /// </summary>
        /// <remarks>
        /// This method functions only in the Unity Editor and during play mode. If a required
        /// scene is already loaded, no additional action will be taken for that scene. If
        /// a scene is not loaded, it will be loaded additively in play mode to ensure its
        /// availability as a dependency.
        /// Any components relying on objects from these required scenes should avoid performing
        /// such operations here. Instead, they should use <c>OnEnable</c> or <c>Start</c> to
        /// ensure that objects from dependent scenes are fully loaded and accessible.
        /// </remarks>
        private void Awake()
        {
            if (!Application.isEditor)
            {
                Destroy(this);
                return;
            }
            
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                return;
            }
        
            foreach (var sceneAsset in requiredScenes)
            {
                if (!sceneAsset) continue;
        
                string sceneName = sceneAsset.name;
                Scene scene = SceneManager.GetSceneByName(sceneName);
        
                if (scene.isLoaded)
                {
                    continue;
                }
                
                Debug.Log($"[SceneDependenciesLoader] Loading missing scene: {sceneName}");
                EditorSceneManager.LoadSceneInPlayMode(AssetDatabase.GetAssetPath(sceneAsset), new LoadSceneParameters(LoadSceneMode.Additive));
            }
#endif
        }
    }
}
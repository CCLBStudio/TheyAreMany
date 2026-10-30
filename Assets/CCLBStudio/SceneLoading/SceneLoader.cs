using CCLBStudio.EventBus;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CCLBStudio.SceneLoading
{
    public static class SceneLoader
    {
        public static async Awaitable ReloadActiveScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            if (!IsSceneValid(activeScene))
            {
                return;
            }
            
            string sceneName = activeScene.name;
            await UnloadSceneAsync(sceneName);
            await LoadSceneAdditiveAsync(sceneName);
            
            var reloadedScene = SceneManager.GetSceneByName(sceneName);
            if (!IsSceneValid(reloadedScene))
            {
                return;
            }
            
            SceneManager.SetActiveScene(reloadedScene);
        }

        public static async Awaitable LoadSceneAdditiveAsync(string sceneName)
        {
            EvtBus.Raise(new WillLoadScene(sceneName));
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            EvtBus.Raise(new SceneLoaded(sceneName));
        }
        
        public static async Awaitable UnloadSceneAsync(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!IsSceneValid(scene))
            {
                return;
            }
            
            if (!scene.isLoaded)
            {
                Debug.LogWarning($"Scene '{sceneName}' is not loaded. Cannot unload.");
                return;
            }
            
            EvtBus.Raise(new WillUnloadScene(sceneName));
            await SceneManager.UnloadSceneAsync(scene);
            EvtBus.Raise(new SceneUnloaded(sceneName));
        }
        
        private static bool IsSceneValid(Scene scene)
        {
            if (scene.IsValid())
            {
                return true;
            }
            Debug.LogError($"Scene '{scene.name}' is not valid. Make sure the scene is added to the build settings.");
            return false;
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

namespace Systems.ScriptableBehaviours.Actions
{
    [CreateAssetMenu(menuName = "Theramor/Systems/Scriptable Behaviours/Scene/Unload", fileName = "NewUnloadSceneAction")]
    public class UnloadSceneScriptableAction : ScriptableAction
    {
        [SerializeField] private string sceneName;
        
        public override void Execute()
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }
    }
}

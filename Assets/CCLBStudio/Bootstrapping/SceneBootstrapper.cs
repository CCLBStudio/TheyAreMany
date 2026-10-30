using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

namespace CCLBStudio.Bootstrapping
{
    [DefaultExecutionOrder(-10)]
    public class SceneBootstrapper : SerializedMonoBehaviour
    {
        [SerializeField] private IBootstrapped[] toBootstrap;

        private void Awake()
        {
            foreach (var b in toBootstrap.OrderBy(b => b.ExecutionOrder))
            {
                b.Initialize();
            }
        }

        #region Editor
        #if UNITY_EDITOR
        
        [Button("Find All Bootstrapped Objects In Scene")]
        private void FindAllBootstrappedInScene()
        {
            toBootstrap ??= Array.Empty<IBootstrapped>();
            HashSet<IBootstrapped> alreadyInList = new HashSet<IBootstrapped>(toBootstrap);
            
            var sceneBootstrap = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include)
                .OfType<IBootstrapped>()
                .Where(b => !alreadyInList.Contains(b))
                .ToArray();
            
            if(sceneBootstrap.Length == 0)
            {
                return;
            }
            
            toBootstrap = toBootstrap.Concat(sceneBootstrap).ToArray();
            SetDirtyAndSave();
        }
        
        [Button("Order By Execution Order")]
        private void OrderByExecutionOrder()
        {
            toBootstrap = toBootstrap.OrderBy(b => b.ExecutionOrder).ToArray();
            SetDirtyAndSave();
        }
        
        private static void SetDirtyAndSave()
        {
            EditorUtility.SetDirty(Selection.activeGameObject);
            var openScenes = GetOpenScenes();
            EditorSceneManager.SaveModifiedScenesIfUserWantsTo(openScenes);
        }
        
        private static Scene[] GetOpenScenes()
        {
            int count = SceneManager.sceneCount;
            var result = new List<Scene>();
            for (int i = 0; i < count; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                result.Add(scene);
            }
            
            return result.ToArray();
        }
        
        #endif
        #endregion
    }
}
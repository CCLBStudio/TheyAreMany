using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CCLBStudio.ScriptablePooling
{
    [CreateAssetMenu(menuName = "CCLB Studio/Scriptable Pooling/Pool", fileName = "NewScriptablePool")]
    public class ScriptablePool : ScriptableObject
    {
        [Header("Pool Objects Settings")]
        [Tooltip("The prefab to instantiate, that will be part of the object pool. This prefab should have a script deriving from the IScriptablePooledObject interface.")]
        [SerializeField] private GameObject pooledObjectPrefab;
        [Tooltip("The initial quantity of pooled objects to instantiate during the Initialize method. If this quantity is not enough, other objects will be automatically spawned.")]
        [Min(1)][SerializeField] private int quantityToInstantiate = 10;
        [Tooltip("If TRUE, automatically disable the pooled object when it is instantiated.")]
        [SerializeField] private bool disableObjectOnCreation;
        [Tooltip("If TRUE, automatically disable the pooled object when it is released.")]
        [SerializeField] private bool disableObjectOnRelease;
        [Tooltip("If TRUE, automatically enable the pooled object when it is requested.")]
        [SerializeField] private bool enableObjectOnRequest;

        [NonSerialized] private Transform _poolContainer;
        [NonSerialized] private bool _init;
        [NonSerialized] private Dictionary<IScriptablePooledObject, GameObject> _available;
        [NonSerialized] private Dictionary<IScriptablePooledObject, GameObject> _inUse;

        #region Initialization

        public void Initialize()
        {
            if (_init)
            {
                Debug.Log($"Pool {name} has already been initialized.");
                return;
            }

            _init = true;
            _available = new Dictionary<IScriptablePooledObject, GameObject>(quantityToInstantiate);
            _inUse = new Dictionary<IScriptablePooledObject, GameObject>(quantityToInstantiate);
            
            CreatePoolContainer();
            for (int i = 0; i < quantityToInstantiate; i++)
            { 
                CreatePooledObject();
            }
        }

        private void CreatePoolContainer()
        {
            if (_poolContainer)
            {
                Debug.Log($"Container for pool {name} has already been created.");
                return;
            }

            _poolContainer = new GameObject($"{name}-Container").transform;
        }

        #endregion

        #region Pooled Object Mehtods

        private void CreatePooledObject()
        {
            if (!pooledObjectPrefab)
            {
                Debug.LogError("Pool object prefab is null !");
                return;
            }
            
            var obj = Instantiate(pooledObjectPrefab, _poolContainer);
            if (!obj.TryGetComponent(out IScriptablePooledObject spo))
            {
                Debug.LogWarning($"Pool object {pooledObjectPrefab.name} does not have an attached script that derives from {nameof(IScriptablePooledObject)}. Adding the {nameof(DefaultPooledObject)} component.");
                spo = obj.AddComponent<DefaultPooledObject>();
            }

            spo.Pool = this;
            _available[spo] = obj;
            spo.OnObjectCreated();
            obj.SetActive(!disableObjectOnCreation);
        }

        public IScriptablePooledObject RequestObject()
        {
            if (!_init)
            {
                Debug.LogWarning($"Pool {name} has not been initialized, performing an auto initialize.");
                Initialize();
            }
            
            if (_available.Count <= 0)
            {
                CreatePooledObject();
            }

            var key = _available.Keys.First();
            var obj = _available[key];
            
            _available.Remove(key);
            _inUse[key] = obj;

            obj.SetActive(enableObjectOnRequest);
            key.OnObjectRequested();

            return key;
        }

        public T RequestObjectAs<T>() where T : IScriptablePooledObject
        {
            if (!_init)
            {
                Debug.LogWarning($"Pool {name} has not been initialized, performing an auto initialize.");
                Initialize();
            }

            if (_available.Count <= 0)
            {
                CreatePooledObject();
            }

            var key = _available.Keys.First();

            if(key is not T casted)
            {
                Debug.LogError($"Pool {name} does not contains pooled objects of type {nameof(T)}");
                return default(T);
            }

            var obj = _available[key];

            _available.Remove(key);
            _inUse[key] = obj;

            obj.SetActive(enableObjectOnRequest);
            key.OnObjectRequested();

            return casted;
        }

        public void ReleaseObject(IScriptablePooledObject pooledObject)
        {
            if (!_init)
            {
                Debug.LogWarning($"Pool {name} has not been initialized, performing an auto initialize.");
                Initialize();
            }
            
            if (!_inUse.ContainsKey(pooledObject))
            {
                Debug.LogError("You tried to release a pooled object that is not currently used.");
                return;
            }

            var obj = _inUse[pooledObject];
            _inUse.Remove(pooledObject);
            _available[pooledObject] = obj;
            pooledObject.OnObjectReleased();
            
            obj.SetActive(!disableObjectOnRelease);
        }

        #endregion
    }
}

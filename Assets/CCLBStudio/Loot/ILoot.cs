using System;
using CCLBStudio.ScriptablePooling;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CCLBStudio.Loot
{
    public interface ILoot
    {
        public string GetId();
        public GameObject Spawn(Vector3 position, Quaternion rotation);
    }
    
    [Serializable]
    public class PrefabLoot : ILoot
    {
        [SerializeField] private GameObject prefab;

        public string GetId()
        {
            return prefab ? prefab.name : "null ref";
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            if(!prefab)
            {
                Debug.LogError("PrefabLoot: Prefab is not assigned.");
                return null;
            }
            
            return Object.Instantiate(prefab, position, rotation);
        }
    }

    [Serializable]
    public class PooledLoot : ILoot
    {
        [SerializeField] private ScriptablePool pool;

        public string GetId()
        {
            return pool ? pool.name : "null ref";
        }

        public GameObject Spawn(Vector3 position, Quaternion rotation)
        {
            if (!pool)
            {
                Debug.LogError("PooledLoot: Pool is not assigned.");
                return null;
            }
            
            var go = pool.RequestObjectAsGo();
            go.transform.SetPositionAndRotation(position, rotation);
            return go;
        }
    }
}
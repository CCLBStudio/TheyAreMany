using System;
using System.Collections.Generic;
using CCLBStudio.Bootstrapping;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Player
{
    [Serializable]
    public class PlayerSpawner : IBootstrapped
    {
        [SerializeField] private List<PlayerSpawnData> spawnData = new();
        public void Initialize()
        {
            foreach (var data in spawnData)
            {
                if(!data.playerPrefab || !data.spawnPoint)
                {
                    Debug.LogError($"Missing data for player spawn, skipping.");
                    continue;
                }
                
                Object.Instantiate(data.playerPrefab, data.spawnPoint.position, data.spawnPoint.rotation);
            }
        }

        [Serializable]
        private struct PlayerSpawnData
        {
            public PlayerFacade playerPrefab;
            public Transform spawnPoint;
        }
    }
}
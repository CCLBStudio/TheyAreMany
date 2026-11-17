using System;
using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    /// <summary>
    /// Contract between the wave system and a spawned enemy, whatever the way it is instantiated (pool, factory...).
    /// </summary>
    public interface IWaveEnemy
    {
        GameObject GameObject { get; }

        /// <summary>
        /// True between the moment the enemy is provided and the moment it is despawned.
        /// </summary>
        bool IsSpawned { get; }

        /// <summary>
        /// Must be raised once the enemy is despawned (killed, released to its pool...) so the wave can track completion.
        /// </summary>
        event Action<IWaveEnemy> Despawned;

        /// <summary>
        /// Called by the <see cref="WaveRunner"/> right after <see cref="WaveEnemyProvider.Provide"/>.
        /// The implementation is responsible for placing itself at <see cref="WaveEnemySpawnContext.Pose"/> and activating its GameObject.
        /// </summary>
        void OnWaveSpawn(in WaveEnemySpawnContext context);

        /// <summary>
        /// Removes the enemy from the game (for example by releasing it to its pool). Must raise <see cref="Despawned"/>.
        /// </summary>
        void Despawn();
    }

    public readonly struct WaveEnemySpawnContext
    {
        public WaveEnemyDefinition Definition { get; }
        public int WaveNumber { get; }
        public int SpawnIndex { get; }
        public Pose Pose { get; }

        public WaveEnemySpawnContext(WaveEnemyDefinition definition, int waveNumber, int spawnIndex, Pose pose)
        {
            Definition = definition;
            WaveNumber = waveNumber;
            SpawnIndex = spawnIndex;
            Pose = pose;
        }
    }
}

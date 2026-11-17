using System;
using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    public abstract class WaveSpawnLocation : MonoBehaviour
    {
        public abstract Pose GetSpawnPose(in WaveSpawnRequest request);
    }

    public readonly struct WaveSpawnRequest
    {
        public WavePlan Wave { get; }
        /// <summary>Index of the spawn in <see cref="WavePlan.Spawns"/>.</summary>
        public int SpawnIndex { get; }
        /// <summary>
        /// Index of this spawn among the spawns handled by the receiving location. Equals <see cref="SpawnIndex"/>
        /// unless a <see cref="WaveSpawnLocationRouter"/> dispatches spawns between several locations.
        /// Use it for cyclic selections.
        /// </summary>
        public int LocalIndex { get; }
        public WaveEnemyDefinition Enemy => Wave.Spawns[SpawnIndex].Enemy;

        public WaveSpawnRequest(WavePlan wave, int spawnIndex) : this(wave, spawnIndex, spawnIndex)
        {
        }

        public WaveSpawnRequest(WavePlan wave, int spawnIndex, int localIndex)
        {
            if (wave == null)
                throw new ArgumentNullException(nameof(wave));
            if (spawnIndex < 0 || spawnIndex >= wave.Spawns.Count)
                throw new ArgumentOutOfRangeException(nameof(spawnIndex));
            if (localIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(localIndex));
            Wave = wave;
            SpawnIndex = spawnIndex;
            LocalIndex = localIndex;
        }
    }

    public enum WaveSpawnPointSelection
    {
        RoundRobin,
        Random
    }
}

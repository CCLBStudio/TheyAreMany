using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    public abstract class WaveSpawnLocation : MonoBehaviour
    {
        public abstract Pose GetSpawnPose(WavePlan wave, int spawnIndex);
    }

    public enum WaveSpawnPointSelection
    {
        RoundRobin,
        Random
    }
}

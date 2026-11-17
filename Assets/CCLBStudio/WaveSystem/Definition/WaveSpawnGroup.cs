using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    /// <summary>
    /// Key shared by enemy definitions that must appear at the same spawn points (e.g. "Flying", "Boss").
    /// Mapped to a scene <see cref="WaveSpawnLocation"/> by a <see cref="WaveSpawnLocationRouter"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "CCLB Studio/Wave System/Spawn Group", fileName = "WaveSpawnGroup")]
    public sealed class WaveSpawnGroup : ScriptableObject
    {
    }
}

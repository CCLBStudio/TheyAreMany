using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    /// <summary>
    /// Supplies the enemy instances spawned by the <see cref="WaveRunner"/>.
    /// </summary>
    public abstract class WaveEnemyProvider : ScriptableObject
    {
        /// <summary>
        /// Called once per runner before the first wave using this provider starts (pool prewarm, etc.).
        /// </summary>
        public virtual void Prepare()
        {
        }

        /// <summary>
        /// Returns a spawned enemy (<see cref="IWaveEnemy.IsSpawned"/> = true), not yet placed.
        /// </summary>
        public abstract IWaveEnemy Provide();
    }
}

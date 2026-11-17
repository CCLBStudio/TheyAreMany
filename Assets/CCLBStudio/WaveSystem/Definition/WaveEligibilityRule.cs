using System.Collections.Generic;
using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    public abstract class WaveEligibilityRule : ScriptableObject
    {
        public abstract bool CanSpawn(WaveEnemyDefinition enemy, WaveGenerationContext context);
    }

    public sealed class WaveGenerationContext
    {
        public int WaveNumber { get; }
        /// <summary>X value used to evaluate the curves for this wave (see <see cref="WaveGenerationSettings.CurveAxis"/>).</summary>
        public float CurveTime { get; }
        public int RemainingBudget { get; internal set; }
        public int RemainingSlots { get; internal set; }

        private readonly IReadOnlyDictionary<WaveEnemyDefinition, int> runCounts;
        private readonly IReadOnlyDictionary<WaveEnemyDefinition, int> waveCounts;

        internal WaveGenerationContext(int waveNumber, float curveTime, int budget, int slots,
            IReadOnlyDictionary<WaveEnemyDefinition, int> runCounts,
            IReadOnlyDictionary<WaveEnemyDefinition, int> waveCounts)
        {
            WaveNumber = waveNumber;
            CurveTime = curveTime;
            RemainingBudget = budget;
            RemainingSlots = slots;
            this.runCounts = runCounts;
            this.waveCounts = waveCounts;
        }

        public int GetRunCount(WaveEnemyDefinition enemy)
        {
            return runCounts.GetValueOrDefault(enemy, 0);
        }

        public int GetWaveCount(WaveEnemyDefinition enemy)
        {
            return waveCounts.GetValueOrDefault(enemy, 0);
        }
    }
}

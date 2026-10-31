using System;
using CCLBStudio.ScriptablePooling;
using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    [CreateAssetMenu(menuName = "CCLB Studio/Wave System/Enemy", fileName = "WaveEnemy")]
    public sealed class WaveEnemyDefinition : ScriptableObject
    {
        public ScriptablePool Pool => pool;
        public int Cost => cost;
        public int FirstWave => firstWave;
        public int LastWave => lastWave;
        public int MaxPerWave => maxPerWave;
        public int MaxPerRun => maxPerRun;

        [SerializeField] private ScriptablePool pool;
        [SerializeField, Min(1)] private int cost = 1;
        [SerializeField, Min(1)] private int firstWave = 1;
        [Tooltip("0 means no upper wave limit.")]
        [SerializeField, Min(0)] private int lastWave;
        [Tooltip("0 means unlimited.")]
        [SerializeField, Min(0)] private int maxPerWave;
        [Tooltip("0 means unlimited across successfully generated waves in one run.")]
        [SerializeField, Min(0)] private int maxPerRun;
        [Tooltip("X = wave number (starting at 1), Y = selection weight. Non-positive weights exclude random selection.")]
        [SerializeField] private AnimationCurve preference = AnimationCurve.Constant(1f, 20f, 1f);
        [SerializeField] private WaveEligibilityRule[] rules = Array.Empty<WaveEligibilityRule>();

        public bool CanSpawn(WaveGenerationContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (context.WaveNumber < firstWave || (lastWave > 0 && context.WaveNumber > lastWave))
                return false;
            if (maxPerWave > 0 && context.GetWaveCount(this) >= maxPerWave)
                return false;
            if (maxPerRun > 0 && (long)context.GetRunCount(this) + context.GetWaveCount(this) >= maxPerRun)
                return false;

            foreach (var rule in rules)
            {
                if (!rule.CanSpawn(this, context))
                    return false;
            }

            return true;
        }

        public float GetPreference(int waveNumber)
        {
            float weight = preference.Evaluate(waveNumber);
            if (float.IsNaN(weight) || float.IsInfinity(weight))
                throw new InvalidOperationException($"Enemy '{name}' has a non-finite preference at wave {waveNumber}.");
            return Mathf.Max(0f, weight);
        }

        internal void Validate()
        {
            if (cost < 1 || firstWave < 1 || lastWave < 0 || (lastWave > 0 && lastWave < firstWave)
                || maxPerWave < 0 || maxPerRun < 0 || preference == null || rules == null)
                throw new InvalidOperationException($"Enemy '{name}' has invalid wave settings.");

            foreach (var rule in rules)
            {
                if (rule == null)
                    throw new InvalidOperationException($"Enemy '{name}' has a missing eligibility rule.");
            }
        }
    }
}

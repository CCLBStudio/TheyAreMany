using System;
using System.Collections.Generic;
using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    [CreateAssetMenu(menuName = "CCLB Studio/Wave System/Generation Settings", fileName = "WaveGenerationSettings")]
    public sealed class WaveGenerationSettings : ScriptableObject
    {
        public IReadOnlyList<WaveEnemyDefinition> Enemies => Array.AsReadOnly(enemies);
        public IReadOnlyList<MandatoryWaveEnemy> MandatoryEnemies => Array.AsReadOnly(mandatoryEnemies);
        public float SpawnDuration => spawnDuration;
        public float SpawnTimingJitter => spawnTimingJitter;
        public int WaveCount => waveCount;
        public WaveCurveAxis CurveAxis => curveAxis;
        public int ReferenceWaveCount => referenceWaveCount;

        [Tooltip("Number of waves in a sequence. 0 means unlimited.")]
        [SerializeField, Min(0)] private int waveCount;
        [Tooltip("X axis of every curve (budget, enemy limit, enemy preferences).\n" +
                 "WaveNumber: X = wave number (1, 2, 3...).\n" +
                 "NormalizedProgress: X = 0 at wave 1 and 1 at the last wave (Wave Count, or Reference Wave Count when unlimited).")]
        [SerializeField] private WaveCurveAxis curveAxis = WaveCurveAxis.WaveNumber;
        [Tooltip("Wave mapped to X = 1 when Curve Axis is NormalizedProgress and Wave Count is 0 (unlimited). " +
                 "Later waves evaluate beyond 1 and follow the curve's post-wrap mode.")]
        [SerializeField, Min(2)] private int referenceWaveCount = 50;
        [SerializeField] private WaveEnemyDefinition[] enemies = Array.Empty<WaveEnemyDefinition>();
        [Tooltip("X = curve axis (see Curve Axis), Y = integer budget (rounded down).")]
        [SerializeField] private AnimationCurve budget = AnimationCurve.Linear(1f, 10f, 20f, 100f);
        [Tooltip("X = curve axis (see Curve Axis), Y = maximum enemy count (rounded down).")]
        [SerializeField] private AnimationCurve enemyLimit = AnimationCurve.Linear(1f, 5f, 20f, 50f);
        [Tooltip("Time between the first and last spawn. Entries are shuffled before being distributed over this duration.")]
        [SerializeField, Min(0f)] private float spawnDuration = 10f;
        [Tooltip("0 = evenly spaced spawns. 1 = each interval varies between 0 and twice the average interval. " +
                 "The first spawn stays at 0 and the last one at Spawn Duration. Seeded, so deterministic.")]
        [SerializeField, Range(0f, 1f)] private float spawnTimingJitter;
        [SerializeField] private MandatoryWaveEnemy[] mandatoryEnemies = Array.Empty<MandatoryWaveEnemy>();

        internal int GetBudget(int waveNumber) => EvaluateInteger(budget, waveNumber, 0, "budget");
        internal int GetEnemyLimit(int waveNumber) => EvaluateInteger(enemyLimit, waveNumber, 1, "enemy limit");

        /// <summary>
        /// Converts a wave number into the X value used to evaluate every curve of the system.
        /// </summary>
        public float GetCurveTime(int waveNumber)
        {
            if (curveAxis == WaveCurveAxis.WaveNumber)
                return waveNumber;
            int lastWave = waveCount > 0 ? waveCount : referenceWaveCount;
            return lastWave <= 1 ? 0f : (waveNumber - 1) / (float)(lastWave - 1);
        }

        internal void Validate()
        {
            if (enemies == null || enemies.Length == 0 || mandatoryEnemies == null || budget == null
                || enemyLimit == null || float.IsNaN(spawnDuration) || float.IsInfinity(spawnDuration)
                || spawnDuration < 0f || waveCount < 0 || float.IsNaN(spawnTimingJitter)
                || spawnTimingJitter < 0f || spawnTimingJitter > 1f || referenceWaveCount < 2
                || (curveAxis != WaveCurveAxis.WaveNumber && curveAxis != WaveCurveAxis.NormalizedProgress))
                throw new InvalidOperationException($"Generation settings '{name}' are incomplete or invalid.");

            var unique = new HashSet<WaveEnemyDefinition>();
            foreach (var enemy in enemies)
            {
                if (enemy == null || !unique.Add(enemy))
                    throw new InvalidOperationException($"Generation settings '{name}' contain a missing or duplicate enemy.");
                enemy.Validate();
            }

            foreach (var mandatory in mandatoryEnemies)
            {
                if (mandatory == null || mandatory.Enemy == null || !unique.Contains(mandatory.Enemy)
                    || mandatory.FirstWave < 1 || mandatory.RepeatEvery < 0 || mandatory.Count < 1
                    || mandatory.LastWave < 0
                    || (mandatory.LastWave > 0 && mandatory.LastWave < mandatory.FirstWave))
                    throw new InvalidOperationException($"Generation settings '{name}' contain an invalid mandatory enemy rule.");
            }
        }

        private int EvaluateInteger(AnimationCurve curve, int waveNumber, int minimum, string label)
        {
            float value = curve.Evaluate(GetCurveTime(waveNumber));
            if (float.IsNaN(value) || float.IsInfinity(value) || value < minimum || (double)value > int.MaxValue)
                throw new InvalidOperationException($"Settings '{name}' have an invalid {label} at wave {waveNumber}.");
            return Mathf.FloorToInt(value);
        }
    }

    public enum WaveCurveAxis
    {
        WaveNumber,
        NormalizedProgress
    }

    [Serializable]
    public sealed class MandatoryWaveEnemy
    {
        public WaveEnemyDefinition Enemy => enemy;
        public int FirstWave => firstWave;
        public int LastWave => lastWave;
        public int RepeatEvery => repeatEvery;
        public int Count => count;

        [SerializeField] private WaveEnemyDefinition enemy;
        [SerializeField, Min(1)] private int firstWave = 1;
        [Tooltip("0 means no upper wave limit.")]
        [SerializeField, Min(0)] private int lastWave;
        [Tooltip("0 = only First Wave. For example, First Wave = 5 and Repeat Every = 5 selects waves 5, 10, 15...")]
        [SerializeField, Min(0)] private int repeatEvery;
        [SerializeField, Min(1)] private int count = 1;

        public bool AppliesTo(int waveNumber)
        {
            return waveNumber >= firstWave && (lastWave == 0 || waveNumber <= lastWave)
                && (repeatEvery == 0 ? waveNumber == firstWave : (waveNumber - firstWave) % repeatEvery == 0);
        }
    }
}

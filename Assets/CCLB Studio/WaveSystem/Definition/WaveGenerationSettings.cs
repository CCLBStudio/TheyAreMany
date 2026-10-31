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

        [SerializeField] private WaveEnemyDefinition[] enemies = Array.Empty<WaveEnemyDefinition>();
        [Tooltip("X = wave number, Y = integer budget (rounded down).")]
        [SerializeField] private AnimationCurve budget = AnimationCurve.Linear(1f, 10f, 20f, 100f);
        [Tooltip("X = wave number, Y = maximum enemy count (rounded down).")]
        [SerializeField] private AnimationCurve enemyLimit = AnimationCurve.Linear(1f, 5f, 20f, 50f);
        [Tooltip("Time between the first and last spawn. Entries are shuffled before being distributed evenly.")]
        [SerializeField, Min(0f)] private float spawnDuration = 10f;
        [SerializeField] private MandatoryWaveEnemy[] mandatoryEnemies = Array.Empty<MandatoryWaveEnemy>();

        internal int GetBudget(int waveNumber) => EvaluateInteger(budget, waveNumber, 0, "budget");
        internal int GetEnemyLimit(int waveNumber) => EvaluateInteger(enemyLimit, waveNumber, 1, "enemy limit");

        internal void Validate()
        {
            if (enemies == null || enemies.Length == 0 || mandatoryEnemies == null || budget == null
                || enemyLimit == null || float.IsNaN(spawnDuration) || float.IsInfinity(spawnDuration)
                || spawnDuration < 0f)
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
            float value = curve.Evaluate(waveNumber);
            if (float.IsNaN(value) || float.IsInfinity(value) || value < minimum || (double)value > int.MaxValue)
                throw new InvalidOperationException($"Settings '{name}' have an invalid {label} at wave {waveNumber}.");
            return Mathf.FloorToInt(value);
        }
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

using System;
using System.Collections.Generic;

namespace CCLBStudio.WaveSystem
{
    public sealed class ProceduralWaveGenerator
    {
        public int NextWaveNumber { get; private set; } = 1;

        private readonly WaveGenerationSettings settings;
        private readonly int seed;
        private readonly Dictionary<WaveEnemyDefinition, int> runCounts = new Dictionary<WaveEnemyDefinition, int>();

        public ProceduralWaveGenerator(WaveGenerationSettings settings, int seed)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));
            settings.Validate();
            this.settings = settings;
            this.seed = seed;
        }

        public int GetRunCount(WaveEnemyDefinition enemy)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));
            return runCounts.TryGetValue(enemy, out int count) ? count : 0;
        }

        public WavePlan GenerateNextWave()
        {
            settings.Validate();
            if (NextWaveNumber == int.MaxValue)
                throw new InvalidOperationException("The wave number limit has been reached.");

            int waveNumber = NextWaveNumber;
            int budget = settings.GetBudget(waveNumber);
            var counts = new Dictionary<WaveEnemyDefinition, int>();
            var context = new WaveGenerationContext(waveNumber, budget, settings.GetEnemyLimit(waveNumber), runCounts, counts);
            var selected = new List<WaveEnemyDefinition>();
            var random = new Random(unchecked(seed * 397 ^ waveNumber));
            var enemies = settings.Enemies;

            // Reserve mandatory enemies first; they override preference, never hard constraints.
            foreach (var mandatory in settings.MandatoryEnemies)
            {
                if (!mandatory.AppliesTo(waveNumber))
                    continue;
                for (int i = 0; i < mandatory.Count; i++)
                {
                    if (!CanAdd(mandatory.Enemy, context))
                        throw new InvalidOperationException(
                            $"Wave {waveNumber} cannot include mandatory enemy '{mandatory.Enemy.name}': check eligibility, caps, budget and slots.");
                    Add(mandatory.Enemy, context, counts, selected);
                }
            }

            var candidates = new List<WaveEnemyDefinition>();
            var weights = new List<double>();
            while (context.RemainingSlots > 0)
            {
                candidates.Clear();
                weights.Clear();
                double totalWeight = 0d;
                foreach (var enemy in enemies)
                {
                    if (!CanAdd(enemy, context))
                        continue;
                    double weight = enemy.GetPreference(waveNumber);
                    if (weight <= 0d)
                        continue;
                    candidates.Add(enemy);
                    weights.Add(weight);
                    totalWeight += weight;
                }

                if (candidates.Count == 0)
                    break;

                double roll = random.NextDouble() * totalWeight;
                int index = candidates.Count - 1;
                for (int i = 0; i < candidates.Count; i++)
                {
                    roll -= weights[i];
                    if (roll < 0d)
                    {
                        index = i;
                        break;
                    }
                }

                Add(candidates[index], context, counts, selected);
            }

            if (selected.Count == 0)
                throw new InvalidOperationException($"Wave {waveNumber} is empty: no eligible, affordable enemy has a positive preference.");

            for (int i = selected.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                var enemy = selected[i];
                selected[i] = selected[j];
                selected[j] = enemy;
            }

            var spawns = new List<WaveSpawn>(selected.Count);
            for (int i = 0; i < selected.Count; i++)
            {
                float delay = selected.Count == 1 ? 0f : settings.SpawnDuration * ((float)i / (selected.Count - 1));
                spawns.Add(new WaveSpawn(selected[i], delay));
            }
            var plan = new WavePlan(waveNumber, budget, budget - context.RemainingBudget, spawns);

            foreach (var pair in counts)
            {
                if ((long)GetRunCount(pair.Key) + pair.Value > int.MaxValue)
                    throw new InvalidOperationException($"Enemy '{pair.Key.name}' has reached the run count limit.");
            }
            foreach (var pair in counts)
                runCounts[pair.Key] = GetRunCount(pair.Key) + pair.Value;
            NextWaveNumber++;
            return plan;
        }

        private static bool CanAdd(WaveEnemyDefinition enemy, WaveGenerationContext context)
        {
            return context.RemainingSlots > 0 && enemy.Cost <= context.RemainingBudget && enemy.CanSpawn(context);
        }

        private static void Add(WaveEnemyDefinition enemy, WaveGenerationContext context,
            Dictionary<WaveEnemyDefinition, int> counts, List<WaveEnemyDefinition> selected)
        {
            counts.TryGetValue(enemy, out int count);
            counts[enemy] = count + 1;
            context.RemainingBudget -= enemy.Cost;
            context.RemainingSlots--;
            selected.Add(enemy);
        }
    }
}

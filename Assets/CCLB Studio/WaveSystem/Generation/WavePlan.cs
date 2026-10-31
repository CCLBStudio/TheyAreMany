using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CCLBStudio.WaveSystem
{
    public readonly struct WaveSpawn
    {
        public WaveEnemyDefinition Enemy { get; }
        public float Delay { get; }

        internal WaveSpawn(WaveEnemyDefinition enemy, float delay)
        {
            Enemy = enemy;
            Delay = delay;
        }
    }

    public sealed class WavePlan
    {
        public int WaveNumber { get; }
        public int Budget { get; }
        public int SpentBudget { get; }
        public IReadOnlyList<WaveSpawn> Spawns { get; }

        internal WavePlan(int waveNumber, int budget, int spentBudget, List<WaveSpawn> spawns)
        {
            WaveNumber = waveNumber;
            Budget = budget;
            SpentBudget = spentBudget;
            Spawns = new ReadOnlyCollection<WaveSpawn>(spawns.ToArray());
        }
    }
}

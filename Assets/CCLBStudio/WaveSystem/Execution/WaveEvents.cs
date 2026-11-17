using CCLBStudio.EventBus;

namespace CCLBStudio.WaveSystem
{
    /// <summary>
    /// Raised when a wave begins. <see cref="TotalWaves"/> = 0 means an unlimited sequence.
    /// </summary>
    public readonly struct WaveStartedEvent : IEvent
    {
        public WaveRunner Runner { get; }
        public WavePlan Wave { get; }
        public int TotalWaves { get; }

        public WaveStartedEvent(WaveRunner runner, WavePlan wave, int totalWaves)
        {
            Runner = runner;
            Wave = wave;
            TotalWaves = totalWaves;
        }
    }

    /// <summary>
    /// Raised after an enemy has been provided and placed.
    /// </summary>
    public readonly struct WaveEnemySpawnedEvent : IEvent
    {
        public WaveRunner Runner { get; }
        public WavePlan Wave { get; }
        public IWaveEnemy Enemy { get; }
        public WaveEnemySpawnContext Context { get; }

        public WaveEnemySpawnedEvent(WaveRunner runner, WavePlan wave, IWaveEnemy enemy, WaveEnemySpawnContext context)
        {
            Runner = runner;
            Wave = wave;
            Enemy = enemy;
            Context = context;
        }
    }

    /// <summary>
    /// Raised when every spawn of a wave has been executed and every enemy has been despawned.
    /// </summary>
    public readonly struct WaveCompletedEvent : IEvent
    {
        public WaveRunner Runner { get; }
        public WavePlan Wave { get; }
        public int TotalWaves { get; }
        public bool IsLastWave { get; }

        public WaveCompletedEvent(WaveRunner runner, WavePlan wave, int totalWaves, bool isLastWave)
        {
            Runner = runner;
            Wave = wave;
            TotalWaves = totalWaves;
            IsLastWave = isLastWave;
        }
    }

    /// <summary>
    /// Raised after the <see cref="WaveCompletedEvent"/> of the last wave of a limited sequence.
    /// </summary>
    public readonly struct AllWavesCompletedEvent : IEvent
    {
        public WaveRunner Runner { get; }
        public int CompletedWaves { get; }

        public AllWavesCompletedEvent(WaveRunner runner, int completedWaves)
        {
            Runner = runner;
            CompletedWaves = completedWaves;
        }
    }

    /// <summary>
    /// Raised when a running wave or sequence is cancelled (<see cref="WaveRunner.StopWaves"/>, runner disabled or execution error).
    /// </summary>
    public readonly struct WaveSequenceStoppedEvent : IEvent
    {
        public WaveRunner Runner { get; }
        public WavePlan InterruptedWave { get; }
        public bool Failed { get; }

        public WaveSequenceStoppedEvent(WaveRunner runner, WavePlan interruptedWave, bool failed)
        {
            Runner = runner;
            InterruptedWave = interruptedWave;
            Failed = failed;
        }
    }
}

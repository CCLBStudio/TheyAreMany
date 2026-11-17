using System;
using System.Collections.Generic;
using CCLBStudio.EventBus;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CCLBStudio.WaveSystem
{
    public sealed class WaveRunner : MonoBehaviour
    {
        public WavePlan CurrentWave { get; private set; }
        public bool IsRunningSequence { get; private set; }
        public int AliveCount => alive.Count;
        public ProceduralWaveGenerator Generator { get; private set; }
        /// <summary>0 means unlimited.</summary>
        public int TotalWaves => settings ? settings.WaveCount : 0;

        [SerializeField] private WaveGenerationSettings settings;
        [SerializeField] private WaveSpawnLocation spawnLocation;
        [SerializeField] private bool startAutomatically;
        [SerializeField] private int seed = 1;
        [SerializeField, Min(0f)] private float delayBetweenWaves = 5f;

        private readonly HashSet<IWaveEnemy> alive = new HashSet<IWaveEnemy>();
        private readonly HashSet<WaveEnemyProvider> preparedProviders = new HashSet<WaveEnemyProvider>();
        private float elapsed;
        private float nextWaveDelay;
        private int nextSpawn;
        private bool stopping;
        private int executionVersion;

        private void Start()
        {
            if (startAutomatically)
                StartSequence();
        }

        public void StartSequence()
        {
            EnsureIdle();
            ValidateExecution();
            Generator = new ProceduralWaveGenerator(settings, seed);
            IsRunningSequence = true;
            try
            {
                BeginWave(Generator.GenerateNextWave());
            }
            catch
            {
                Stop(true);
                throw;
            }
        }

        public void StartWave(WavePlan plan)
        {
            EnsureIdle();
            ValidateExecution();
            try
            {
                BeginWave(plan);
            }
            catch
            {
                Stop(true);
                throw;
            }
        }

        public void StopWaves()
        {
            Stop(false);
        }

        private void Stop(bool failed)
        {
            if (stopping)
                return;
            stopping = true;
            var interrupted = CurrentWave;
            bool wasRunning = interrupted != null || IsRunningSequence;
            executionVersion++;
            IsRunningSequence = false;
            CurrentWave = null;
            var leased = new List<IWaveEnemy>(alive);
            alive.Clear();
            try
            {
                foreach (var enemy in leased)
                {
                    if (IsMissing(enemy))
                        continue;
                    enemy.Despawned -= OnEnemyDespawned;
                    if (!enemy.IsSpawned)
                        continue;
                    try
                    {
                        enemy.Despawn();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception, enemy as Object);
                    }
                }
            }
            finally
            {
                stopping = false;
            }

            if (wasRunning)
                EvtBus.Raise(new WaveSequenceStoppedEvent(this, interrupted, failed));
        }

        private void Update()
        {
            try
            {
                if (CurrentWave == null)
                {
                    if (!IsRunningSequence)
                        return;
                    nextWaveDelay -= Time.deltaTime;
                    if (nextWaveDelay <= 0f)
                        BeginWave(Generator.GenerateNextWave());
                    return;
                }

                int version = executionVersion;
                var plan = CurrentWave;
                elapsed += Time.deltaTime;
                while (nextSpawn < plan.Spawns.Count && plan.Spawns[nextSpawn].Delay <= elapsed)
                {
                    int index = nextSpawn++;
                    Spawn(plan, index);
                    if (version != executionVersion)
                        return;
                }

                if (nextSpawn == plan.Spawns.Count && alive.Count == 0)
                    CompleteWave(plan);
            }
            catch (Exception exception)
            {
                Stop(true);
                Debug.LogException(exception, this);
            }
        }

        private void BeginWave(WavePlan plan)
        {
            if (plan == null)
                throw new ArgumentNullException(nameof(plan));
            if (plan.Spawns.Count == 0)
                throw new InvalidOperationException("Cannot execute an empty wave.");
            foreach (var spawn in plan.Spawns)
            {
                if (spawn.Enemy == null || spawn.Enemy.Provider == null)
                    throw new InvalidOperationException($"Wave {plan.WaveNumber} has an enemy without a WaveEnemyProvider.");
                var provider = spawn.Enemy.Provider;
                if (preparedProviders.Contains(provider))
                    continue;
                provider.Prepare();
                preparedProviders.Add(provider);
            }

            executionVersion++;
            CurrentWave = plan;
            nextSpawn = 0;
            elapsed = 0f;
            EvtBus.Raise(new WaveStartedEvent(this, plan, TotalWaves));
        }

        private void Spawn(WavePlan plan, int index)
        {
            Pose pose = spawnLocation.GetSpawnPose(new WaveSpawnRequest(plan, index));
            var definition = plan.Spawns[index].Enemy;
            var enemy = definition.Provider.Provide();
            if (IsMissing(enemy))
                throw new InvalidOperationException($"Provider '{definition.Provider.name}' of '{definition.name}' did not return an enemy.");
            if (!enemy.IsSpawned)
                throw new InvalidOperationException($"Provider '{definition.Provider.name}' returned an enemy that is not spawned.");
            if (!alive.Add(enemy))
                throw new InvalidOperationException($"Provider '{definition.Provider.name}' returned an enemy already owned by this wave.");
            enemy.Despawned += OnEnemyDespawned;
            int version = executionVersion;
            var context = new WaveEnemySpawnContext(definition, plan.WaveNumber, index, pose);
            enemy.OnWaveSpawn(in context);
            if (version == executionVersion && enemy.IsSpawned)
                EvtBus.Raise(new WaveEnemySpawnedEvent(this, plan, enemy, context));
        }

        private void OnEnemyDespawned(IWaveEnemy enemy)
        {
            enemy.Despawned -= OnEnemyDespawned;
            alive.Remove(enemy);
        }

        private void CompleteWave(WavePlan plan)
        {
            CurrentWave = null;
            executionVersion++;
            int totalWaves = TotalWaves;
            bool isLastWave = totalWaves > 0 && plan.WaveNumber >= totalWaves;
            bool sequenceFinished = IsRunningSequence && !Generator.HasNextWave;
            if (sequenceFinished)
                IsRunningSequence = false;
            nextWaveDelay = delayBetweenWaves;
            EvtBus.Raise(new WaveCompletedEvent(this, plan, totalWaves, isLastWave));
            if (sequenceFinished)
                EvtBus.Raise(new AllWavesCompletedEvent(this, plan.WaveNumber));
        }

        private void EnsureIdle()
        {
            if (stopping || CurrentWave != null || IsRunningSequence)
                throw new InvalidOperationException("Stop the current wave sequence before starting another one.");
            if (!isActiveAndEnabled)
                throw new InvalidOperationException("WaveRunner must be active and enabled to start a wave.");
        }

        private void ValidateExecution()
        {
            if (spawnLocation == null || delayBetweenWaves < 0f || float.IsNaN(delayBetweenWaves)
                || float.IsInfinity(delayBetweenWaves))
                throw new InvalidOperationException($"Wave runner '{name}' has invalid execution settings.");
        }

        private static bool IsMissing(IWaveEnemy enemy)
        {
            return enemy is Object unityObject ? !unityObject : enemy == null;
        }

        private void OnDisable()
        {
            StopWaves();
        }
    }
}

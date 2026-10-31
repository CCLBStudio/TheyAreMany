using System;
using System.Collections.Generic;
using CCLBStudio.ScriptablePooling;
using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    public sealed class WaveRunner : MonoBehaviour
    {
        public WavePlan CurrentWave { get; private set; }
        public bool IsRunningSequence { get; private set; }
        public int AliveCount => alive.Count;
        public ProceduralWaveGenerator Generator { get; private set; }

        public event Action<WavePlan> WaveStarted;
        public event Action<WavePlan, WavePooledEnemy> EnemySpawned;
        public event Action<WavePlan> WaveCompleted;

        [SerializeField] private WaveGenerationSettings settings;
        [SerializeField] private WaveSpawnLocation spawnLocation;
        [SerializeField] private bool startAutomatically;
        [SerializeField] private int seed = 1;
        [SerializeField, Min(0f)] private float delayBetweenWaves = 5f;
        [Tooltip("0 means an unlimited sequence.")]
        [SerializeField, Min(0)] private int lastWave;

        private readonly HashSet<WavePooledEnemy> alive = new HashSet<WavePooledEnemy>();
        private readonly HashSet<ScriptablePool> initializedPools = new HashSet<ScriptablePool>();
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
                StopWaves();
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
                StopWaves();
                throw;
            }
        }

        public void StopWaves()
        {
            if (stopping)
                return;
            stopping = true;
            executionVersion++;
            IsRunningSequence = false;
            CurrentWave = null;
            var leased = new List<WavePooledEnemy>(alive);
            alive.Clear();
            try
            {
                foreach (var enemy in leased)
                {
                    if (enemy == null)
                        continue;
                    enemy.Released -= OnEnemyReleased;
                    if (!enemy.IsSpawned)
                        continue;
                    try
                    {
                        enemy.Despawn();
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception, enemy);
                    }
                }
            }
            finally
            {
                stopping = false;
            }
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
                StopWaves();
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
                if (spawn.Enemy == null || spawn.Enemy.Pool == null)
                    throw new InvalidOperationException($"Wave {plan.WaveNumber} has an enemy without a ScriptablePool.");
                var pool = spawn.Enemy.Pool;
                if (initializedPools.Contains(pool))
                    continue;
                pool.Initialize();
                initializedPools.Add(pool);
            }

            executionVersion++;
            CurrentWave = plan;
            nextSpawn = 0;
            elapsed = 0f;
            WaveStarted?.Invoke(plan);
        }

        private void Spawn(WavePlan plan, int index)
        {
            Pose pose = spawnLocation.GetSpawnPose(plan, index);
            var definition = plan.Spawns[index].Enemy;
            var enemy = definition.Pool.RequestObjectAs<WavePooledEnemy>();
            if (enemy == null)
                throw new InvalidOperationException($"Pool for '{definition.name}' did not return a WavePooledEnemy.");
            if (!alive.Add(enemy))
                throw new InvalidOperationException($"Pool for '{definition.name}' returned an enemy already owned by this wave.");
            enemy.Released += OnEnemyReleased;
            enemy.transform.SetPositionAndRotation(pose.position, pose.rotation);
            if (!enemy.gameObject.activeSelf)
                enemy.gameObject.SetActive(true);
            int version = executionVersion;
            enemy.BindToWave(definition, plan.WaveNumber);
            if (version == executionVersion && enemy.IsSpawned)
                EnemySpawned?.Invoke(plan, enemy);
        }

        private void OnEnemyReleased(WavePooledEnemy enemy)
        {
            enemy.Released -= OnEnemyReleased;
            alive.Remove(enemy);
        }

        private void CompleteWave(WavePlan plan)
        {
            CurrentWave = null;
            executionVersion++;
            if (lastWave > 0 && plan.WaveNumber >= lastWave)
                IsRunningSequence = false;
            nextWaveDelay = delayBetweenWaves;
            WaveCompleted?.Invoke(plan);
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
                || float.IsInfinity(delayBetweenWaves) || lastWave < 0)
                throw new InvalidOperationException($"Wave runner '{name}' has invalid execution settings.");
        }

        private void OnDisable()
        {
            StopWaves();
        }
    }
}

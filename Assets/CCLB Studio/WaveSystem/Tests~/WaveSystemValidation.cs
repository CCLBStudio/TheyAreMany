using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CCLBStudio.ScriptablePooling;
using CCLBStudio.WaveSystem;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class WaveSystemValidation
{
    private const string RunningKey = "WaveSystemValidation.Running";
    private static readonly List<Object> created = new List<Object>();

    static WaveSystemValidation()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    public static void Run()
    {
        SessionState.SetBool(RunningKey, true);
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(RunningKey, false))
            return;
        SessionState.SetBool(RunningKey, false);
        RunTests();
    }

    private static void RunTests()
    {
        var tests = new Action[]
        {
            BudgetsEligibilityAndCaps,
            PreferenceCurves,
            WeightedDistribution,
            MandatoryBosses,
            FailedGenerationIsAtomic,
            MandatoryConstraintsAreHard,
            CustomRules,
            InvalidSettings,
            DeterminismAndImmutablePlans,
            PooledExecutionAndCompletion,
            StopAndDisable,
            AutomaticSequence,
            SpawnFailureStopsSequence,
            ReentrantStopFromSpawn,
            SpawnPointsAndRelativeTiming
        };
        try
        {
            foreach (var test in tests)
            {
                test();
                Debug.Log("PASS: " + test.Method.Name);
                Cleanup();
            }
            Debug.Log("WAVE_SYSTEM_VALIDATION_PASSED: " + tests.Length + " tests");
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Cleanup();
            EditorApplication.Exit(1);
        }
    }

    private static void BudgetsEligibilityAndCaps()
    {
        var basic = Enemy("Basic", 2);
        var advanced = Enemy("Advanced", 3);
        Set(advanced, "firstWave", 3);
        Set(advanced, "lastWave", 8);
        Set(advanced, "maxPerWave", 2);
        Set(advanced, "maxPerRun", 6);
        var settings = Settings(11, 5, basic, advanced);
        for (int seed = 0; seed < 80; seed++)
        {
            var generator = new ProceduralWaveGenerator(settings, seed);
            int advancedTotal = 0;
            for (int wave = 1; wave <= 12; wave++)
            {
                var plan = generator.GenerateNextWave();
                int advancedCount = Count(plan, advanced);
                advancedTotal += advancedCount;
                Assert(plan.WaveNumber == wave && plan.Spawns.Count <= 5, "Wave numbering and count cap.");
                Assert(plan.SpentBudget <= 11 && plan.SpentBudget == plan.Spawns.Sum(s => s.Enemy.Cost), "Exact budget.");
                Assert(advancedCount <= 2 && advancedTotal <= 6, "Per-wave and per-run caps.");
                Assert((wave >= 3 && wave <= 8) || advancedCount == 0, "Eligibility window.");
                Assert(generator.GetRunCount(advanced) == advancedTotal, "History matches plans.");
            }
        }
    }

    private static void PreferenceCurves()
    {
        var basic = Enemy("Basic");
        var advanced = Enemy("Advanced");
        Set(basic, "preference", AnimationCurve.Linear(1f, 1f, 2f, 0f));
        Set(advanced, "preference", AnimationCurve.Linear(1f, -1f, 2f, 1f));
        var generator = new ProceduralWaveGenerator(Settings(10, 10, basic, advanced), 3);
        Assert(Count(generator.GenerateNextWave(), basic) == 10, "Early preference excludes complex enemy.");
        Assert(Count(generator.GenerateNextWave(), advanced) == 10, "Later preference excludes basic enemy.");
    }

    private static void WeightedDistribution()
    {
        var basic = Enemy("Basic");
        var advanced = Enemy("Advanced");
        Set(basic, "preference", AnimationCurve.Constant(1f, 2000f, 3f));
        var generator = new ProceduralWaveGenerator(Settings(1, 1, basic, advanced), 19);
        int basicCount = 0;
        for (int i = 0; i < 2000; i++)
            basicCount += Count(generator.GenerateNextWave(), basic);
        float proportion = basicCount / 2000f;
        Assert(proportion > 0.70f && proportion < 0.80f, "3:1 weights approximate 75%, got " + proportion);
    }

    private static void MandatoryBosses()
    {
        var basic = Enemy("Basic");
        var boss = Enemy("Boss", 6);
        Set(boss, "firstWave", 3);
        Set(boss, "maxPerWave", 1);
        Set(boss, "preference", AnimationCurve.Constant(1f, 20f, 0f));
        var settings = Settings(10, 5, basic, boss);
        Set(settings, "mandatoryEnemies", new[] { Mandatory(boss, 3, 3, 1, 6) });
        var generator = new ProceduralWaveGenerator(settings, 5);
        for (int wave = 1; wave <= 9; wave++)
        {
            var plan = generator.GenerateNextWave();
            Assert(Count(plan, boss) == (wave == 3 || wave == 6 ? 1 : 0), "Periodic bounded boss obligation.");
            Assert(plan.SpentBudget <= 10, "Mandatory consumes budget.");
        }
    }

    private static void FailedGenerationIsAtomic()
    {
        var basic = Enemy("Basic");
        var boss = Enemy("Boss", 4);
        Set(boss, "preference", AnimationCurve.Constant(1f, 20f, 0f));
        var mandatory = Mandatory(boss, 2, 0, 3);
        var settings = Settings(10, 10, basic, boss);
        Set(settings, "mandatoryEnemies", new[] { mandatory });
        var generator = new ProceduralWaveGenerator(settings, 4);
        generator.GenerateNextWave();
        int previousCount = generator.GetRunCount(basic);
        ExpectFailure(() => generator.GenerateNextWave());
        Assert(generator.NextWaveNumber == 2 && generator.GetRunCount(boss) == 0
            && generator.GetRunCount(basic) == previousCount, "Failure does not commit partial reservations.");
        Set(mandatory, "count", 1);
        var retried = generator.GenerateNextWave();
        Assert(retried.WaveNumber == 2 && Count(retried, boss) >= 1, "Retry uses same wave.");
    }

    private static void MandatoryConstraintsAreHard()
    {
        var enemy = Enemy("Boss");
        var mandatory = Mandatory(enemy, 1, 1, 1);
        var settings = Settings(1, 1, enemy);
        Set(settings, "mandatoryEnemies", new[] { mandatory });
        Set(enemy, "maxPerRun", 2);
        var generator = new ProceduralWaveGenerator(settings, 1);
        generator.GenerateNextWave();
        generator.GenerateNextWave();
        ExpectFailure(() => generator.GenerateNextWave());
        Assert(generator.GetRunCount(enemy) == 2 && generator.NextWaveNumber == 3, "Mandatory respects run cap.");
        Set(enemy, "firstWave", 2);
        ExpectFailure(() => new ProceduralWaveGenerator(settings, 1).GenerateNextWave());
        Set(enemy, "firstWave", 1);
        Set(enemy, "maxPerWave", 1);
        Set(mandatory, "count", 2);
        Set(settings, "budget", AnimationCurve.Constant(1f, 20f, 10f));
        Set(settings, "enemyLimit", AnimationCurve.Constant(1f, 20f, 10f));
        ExpectFailure(() => new ProceduralWaveGenerator(settings, 1).GenerateNextWave());
        Set(enemy, "maxPerWave", 0);
        Set(settings, "enemyLimit", AnimationCurve.Constant(1f, 20f, 1f));
        ExpectFailure(() => new ProceduralWaveGenerator(settings, 1).GenerateNextWave());
    }

    private static void CustomRules()
    {
        var basic = Enemy("Basic");
        var special = Enemy("Special");
        Set(special, "rules", new WaveEligibilityRule[] { Track(ScriptableObject.CreateInstance<OnePerWaveRule>()) });
        var generator = new ProceduralWaveGenerator(Settings(20, 20, basic, special), 5);
        for (int i = 0; i < 10; i++)
            Assert(Count(generator.GenerateNextWave(), special) <= 1, "Extension rule sees current selections.");
    }

    private static void InvalidSettings()
    {
        var enemy = Enemy("Basic");
        var settings = Settings(5, 5, enemy, enemy);
        ExpectFailure(() => new ProceduralWaveGenerator(settings, 1));
        Set(settings, "enemies", new[] { enemy });
        Set(enemy, "cost", 0);
        ExpectFailure(() => new ProceduralWaveGenerator(settings, 1));
        Set(enemy, "cost", 1);
        Set(enemy, "rules", new WaveEligibilityRule[] { null });
        ExpectFailure(() => new ProceduralWaveGenerator(settings, 1));
        Set(enemy, "rules", Array.Empty<WaveEligibilityRule>());
        Set(settings, "budget", AnimationCurve.Constant(1f, 20f, -1f));
        ExpectFailure(() => new ProceduralWaveGenerator(settings, 1).GenerateNextWave());
        Set(settings, "budget", AnimationCurve.Constant(1f, 20f, 0f));
        ExpectFailure(() => new ProceduralWaveGenerator(settings, 1).GenerateNextWave());
        Set(settings, "budget", AnimationCurve.Constant(1f, 20f, 5f));
        Set(enemy, "preference", AnimationCurve.Constant(1f, 20f, 0f));
        ExpectFailure(() => new ProceduralWaveGenerator(settings, 1).GenerateNextWave());
    }

    private static void DeterminismAndImmutablePlans()
    {
        var settings = Settings(40, 20, Enemy("Basic"), Enemy("Advanced", 3));
        Set(settings, "spawnDuration", 8f);
        var first = new ProceduralWaveGenerator(settings, 42);
        var second = new ProceduralWaveGenerator(settings, 42);
        UnityEngine.Random.InitState(101);
        var before = UnityEngine.Random.state;
        for (int i = 0; i < 12; i++)
        {
            var a = first.GenerateNextWave();
            var b = second.GenerateNextWave();
            Assert(a.Spawns.Select(s => s.Enemy).SequenceEqual(b.Spawns.Select(s => s.Enemy)), "Seeded plans match.");
            Assert(a.Spawns[0].Delay == 0f && a.Spawns[a.Spawns.Count - 1].Delay == 8f, "Timing endpoints.");
            for (int j = 1; j < a.Spawns.Count; j++)
                Assert(a.Spawns[j].Delay >= a.Spawns[j - 1].Delay, "Timing is sorted.");
            Assert(((IList<WaveSpawn>)a.Spawns).IsReadOnly, "Plan exposes no mutable entries.");
        }
        float afterValue = UnityEngine.Random.value;
        UnityEngine.Random.state = before;
        Assert(afterValue == UnityEngine.Random.value, "Generation preserves Unity random state.");
    }

    private static void PooledExecutionAndCompletion()
    {
        var enemy = Enemy("Basic");
        var pool = Pool(enemy);
        var settings = Settings(3, 3, enemy);
        var runner = Runner(settings);
        var plan = new ProceduralWaveGenerator(settings, 2).GenerateNextWave();
        var spawned = new List<WavePooledEnemy>();
        int completed = 0;
        runner.EnemySpawned += (wave, instance) =>
        {
            Assert(instance.Definition == enemy && instance.WaveNumber == wave.WaveNumber, "Spawn context.");
            spawned.Add(instance);
        };
        runner.WaveCompleted += wave => completed++;
        runner.StartWave(plan);
        Invoke(runner, "Update");
        Assert(InUse(pool) == 3 && runner.AliveCount == 3 && spawned.All(s => s.gameObject.activeSelf), "All retrieval uses pool and spawned enemies are active.");
        Assert(completed == 0 && runner.CurrentWave == plan, "Spawning alone does not complete wave.");
        foreach (var instance in spawned)
            instance.Despawn();
        Invoke(runner, "Update");
        Assert(completed == 1 && runner.CurrentWave == null && InUse(pool) == 0, "Release completes wave.");
        runner.StartWave(plan);
        Invoke(runner, "Update");
        Assert(Total(pool) == 3 && runner.AliveCount == 3, "Pooled instances are reused.");
        runner.StopWaves();
        Assert(InUse(pool) == 0 && completed == 1, "Cancellation is not completion.");
    }

    private static void StopAndDisable()
    {
        var enemy = Enemy("Basic");
        var pool = Pool(enemy);
        var settings = Settings(2, 2, enemy);
        var runner = Runner(settings);
        runner.StartSequence();
        Invoke(runner, "Update");
        runner.enabled = false;
        Assert(!runner.IsRunningSequence && runner.CurrentWave == null && runner.AliveCount == 0
            && InUse(pool) == 0, "Disabling returns leases and stops sequence.");
        runner.StopWaves();
        Assert(InUse(pool) == 0, "Stop is idempotent.");
    }

    private static void AutomaticSequence()
    {
        var enemy = Enemy("Basic");
        Pool(enemy);
        var runner = Runner(Settings(1, 1, enemy));
        Set(runner, "lastWave", 2);
        Set(runner, "delayBetweenWaves", 100f);
        runner.EnemySpawned += (plan, instance) => instance.Despawn();
        int completed = 0;
        runner.WaveCompleted += plan => completed++;
        runner.StartSequence();
        Invoke(runner, "Update");
        Assert(completed == 1 && runner.CurrentWave == null && runner.IsRunningSequence, "First wave completes.");
        Invoke(runner, "Update");
        Assert(runner.CurrentWave == null && runner.Generator.NextWaveNumber == 2, "Inter-wave delay enforced.");
        Set(runner, "nextWaveDelay", 0f);
        Invoke(runner, "Update");
        Invoke(runner, "Update");
        Assert(completed == 2 && !runner.IsRunningSequence, "Sequence stops at final wave.");
    }

    private static void SpawnFailureStopsSequence()
    {
        var enemy = Enemy("Basic");
        var pool = Pool(enemy);
        var runner = Runner(Settings(2, 2, enemy));
        var provider = (WaveSpawnPoints)Get(runner, "spawnLocation");
        runner.EnemySpawned += (plan, instance) => Set(provider, "points", Array.Empty<Transform>());
        runner.StartSequence();
        Invoke(runner, "Update");
        Assert(!runner.IsRunningSequence && runner.CurrentWave == null && runner.AliveCount == 0
            && InUse(pool) == 0 && Total(pool) >= 1, "Failure stops and returns earlier spawns.");
    }

    private static void ReentrantStopFromSpawn()
    {
        var enemy = Enemy("Basic");
        var pool = Pool(enemy);
        var runner = Runner(Settings(3, 3, enemy));
        int spawnedCount = 0;
        runner.EnemySpawned += (plan, instance) => { spawnedCount++; runner.StopWaves(); };
        runner.StartSequence();
        Invoke(runner, "Update");
        Assert(spawnedCount == 1 && InUse(pool) == 0 && runner.CurrentWave == null, "Reentrant stop prevents later spawns.");
    }

    private static void SpawnPointsAndRelativeTiming()
    {
        var enemy = Enemy("Basic");
        Pool(enemy);
        var settings = Settings(3, 3, enemy);
        Set(settings, "spawnDuration", 10f);
        var runner = Runner(settings);
        var provider = (WaveSpawnPoints)Get(runner, "spawnLocation");
        var point = Track(new GameObject("SecondPoint")).transform;
        point.position = new Vector3(9f, 3f, 0f);
        Set(provider, "points", new[] { provider.transform, point });
        var plan = new ProceduralWaveGenerator(settings, 1).GenerateNextWave();
        Assert(provider.GetSpawnPose(plan, 1).position == point.position, "Round robin uses second point.");
        var spawned = new List<WavePooledEnemy>();
        runner.EnemySpawned += (wave, instance) => spawned.Add(instance);
        runner.StartWave(plan);
        Invoke(runner, "Update");
        Assert(spawned.Count == 1, "Only first spawn is initially due.");
        spawned[0].Despawn();
        Invoke(runner, "Update");
        Assert(runner.CurrentWave == plan, "Pending spawns prevent early completion.");
        Set(runner, "elapsed", 5f);
        Invoke(runner, "Update");
        Assert(spawned.Count == 2 && spawned[1].transform.position == point.position, "Second spawn at relative time and pose.");
        Set(runner, "elapsed", 10f);
        Invoke(runner, "Update");
        Assert(spawned.Count == 3, "Final spawn scheduled.");
        Set(provider, "points", Array.Empty<Transform>());
        ExpectFailure(() => provider.GetSpawnPose(plan, 0));
        runner.StopWaves();
    }

    private static WaveEnemyDefinition Enemy(string name, int cost = 1)
    {
        var enemy = Track(ScriptableObject.CreateInstance<WaveEnemyDefinition>());
        enemy.name = name;
        Set(enemy, "cost", cost);
        return enemy;
    }

    private static WaveGenerationSettings Settings(int budget, int count, params WaveEnemyDefinition[] enemies)
    {
        var settings = Track(ScriptableObject.CreateInstance<WaveGenerationSettings>());
        Set(settings, "enemies", enemies);
        Set(settings, "budget", AnimationCurve.Constant(1f, 2000f, budget));
        Set(settings, "enemyLimit", AnimationCurve.Constant(1f, 2000f, count));
        Set(settings, "spawnDuration", 0f);
        return settings;
    }

    private static MandatoryWaveEnemy Mandatory(WaveEnemyDefinition enemy, int wave, int repeat, int count, int last = 0)
    {
        var mandatory = new MandatoryWaveEnemy();
        Set(mandatory, "enemy", enemy);
        Set(mandatory, "firstWave", wave);
        Set(mandatory, "repeatEvery", repeat);
        Set(mandatory, "count", count);
        Set(mandatory, "lastWave", last);
        return mandatory;
    }

    private static ScriptablePool Pool(WaveEnemyDefinition enemy)
    {
        var template = Track(new GameObject(enemy.name + "Prefab"));
        template.SetActive(false);
        template.AddComponent<WavePooledEnemy>();
        var pool = Track(ScriptableObject.CreateInstance<ScriptablePool>());
        pool.name = enemy.name + "Pool";
        Set(pool, "pooledObjectPrefab", template);
        Set(pool, "quantityToInstantiate", 1);
        Set(pool, "disableObjectOnCreation", true);
        Set(pool, "disableObjectOnRelease", true);
        Set(enemy, "pool", pool);
        return pool;
    }

    private static int InUse(ScriptablePool pool) => ((System.Collections.ICollection)Get(pool, "_inUse")).Count;
    private static int Total(ScriptablePool pool) => InUse(pool) + ((System.Collections.ICollection)Get(pool, "_available")).Count;

    private static WaveRunner Runner(WaveGenerationSettings settings)
    {
        var provider = Track(new GameObject("SpawnPoints")).AddComponent<WaveSpawnPoints>();
        Set(provider, "points", new[] { provider.transform });
        var runner = Track(new GameObject("WaveRunner")).AddComponent<WaveRunner>();
        Set(runner, "settings", settings);
        Set(runner, "spawnLocation", provider);
        return runner;
    }

    private static int Count(WavePlan plan, WaveEnemyDefinition enemy) => plan.Spawns.Count(s => s.Enemy == enemy);
    private static T Track<T>(T value) where T : Object { created.Add(value); return value; }
    private static object Get(object target, string field) => target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(target);
    private static void Set(object target, string field, object value) => target.GetType().GetField(field, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);
    private static void Invoke(object target, string method) => target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(target, null);
    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new Exception("Assertion failed: " + message);
    }

    private static void ExpectFailure(Action action)
    {
        try { action(); }
        catch (InvalidOperationException) { return; }
        throw new Exception("Expected InvalidOperationException.");
    }

    private static void Cleanup()
    {
        foreach (var value in created)
        {
            if (value is GameObject gameObject && gameObject.TryGetComponent<WaveRunner>(out var runner))
                runner.StopWaves();
        }
        for (int i = created.Count - 1; i >= 0; i--)
        {
            if (created[i] != null)
                Object.DestroyImmediate(created[i]);
        }
        created.Clear();
    }
}

public sealed class OnePerWaveRule : WaveEligibilityRule
{
    public override bool CanSpawn(WaveEnemyDefinition enemy, WaveGenerationContext context)
    {
        return context.GetWaveCount(enemy) < 1;
    }
}

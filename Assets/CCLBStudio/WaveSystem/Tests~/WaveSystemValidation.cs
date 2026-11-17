using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CCLBStudio.EventBus;
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
            WaveCountLimitsGeneration,
            StopRaisesEvent,
            SpawnFailureStopsSequence,
            ReentrantStopFromSpawn,
            SpawnPointsAndRelativeTiming,
            NormalizedCurveAxis,
            SpawnTimingJitter,
            SpawnGroupsRouting
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
        var spawned = new List<TestWaveEnemy>();
        int completed = 0;
        bool contextMatches = true;
        On((in WaveEnemySpawnedEvent e) =>
        {
            var instance = (TestWaveEnemy)e.Enemy;
            contextMatches &= e.Runner == runner && instance.Definition == enemy && instance.WaveNumber == e.Wave.WaveNumber
                && e.Context.Definition == enemy;
            spawned.Add(instance);
        });
        On((in WaveCompletedEvent e) => completed++);
        runner.StartWave(plan);
        Invoke(runner, "Update");
        Assert(contextMatches, "Spawn context.");
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
        var settings = Settings(1, 1, enemy);
        Set(settings, "waveCount", 2);
        var runner = Runner(settings);
        Set(runner, "delayBetweenWaves", 100f);
        On((in WaveEnemySpawnedEvent e) => e.Enemy.Despawn());
        int completed = 0;
        int started = 0;
        int allCompleted = 0;
        bool lastFlagged = false;
        bool totalsMatch = true;
        On((in WaveStartedEvent e) => { started++; totalsMatch &= e.TotalWaves == 2; });
        On((in WaveCompletedEvent e) => { completed++; lastFlagged = e.IsLastWave; });
        On((in AllWavesCompletedEvent e) => { allCompleted++; totalsMatch &= e.CompletedWaves == 2; });
        runner.StartSequence();
        Invoke(runner, "Update");
        Assert(started == 1 && completed == 1 && !lastFlagged && runner.CurrentWave == null && runner.IsRunningSequence, "First wave completes.");
        Invoke(runner, "Update");
        Assert(runner.CurrentWave == null && runner.Generator.NextWaveNumber == 2, "Inter-wave delay enforced.");
        Set(runner, "nextWaveDelay", 0f);
        Invoke(runner, "Update");
        Invoke(runner, "Update");
        Assert(started == 2 && completed == 2 && lastFlagged && allCompleted == 1 && !runner.IsRunningSequence, "Sequence stops at final wave.");
        Set(runner, "nextWaveDelay", 0f);
        Invoke(runner, "Update");
        Assert(started == 2 && allCompleted == 1 && totalsMatch, "No wave after the last one.");
    }

    private static void WaveCountLimitsGeneration()
    {
        var enemy = Enemy("Basic");
        var settings = Settings(1, 1, enemy);
        Set(settings, "waveCount", 3);
        var generator = new ProceduralWaveGenerator(settings, 1);
        for (int i = 0; i < 3; i++)
        {
            Assert(generator.HasNextWave, "Waves remain.");
            generator.GenerateNextWave();
        }
        Assert(!generator.HasNextWave && generator.TotalWaves == 3, "Limit reached.");
        ExpectFailure(() => generator.GenerateNextWave());
        Set(settings, "waveCount", 0);
        var unlimited = new ProceduralWaveGenerator(settings, 1);
        for (int i = 0; i < 50; i++)
            unlimited.GenerateNextWave();
        Assert(unlimited.HasNextWave, "0 means unlimited.");
        Set(settings, "waveCount", -1);
        ExpectFailure(() => new ProceduralWaveGenerator(settings, 1));
    }

    private static void StopRaisesEvent()
    {
        var enemy = Enemy("Basic");
        var pool = Pool(enemy);
        var runner = Runner(Settings(2, 2, enemy));
        int stopped = 0;
        WavePlan interrupted = null;
        bool failed = false;
        On((in WaveSequenceStoppedEvent e) => { stopped++; interrupted = e.InterruptedWave; failed |= e.Failed; });
        runner.StopWaves();
        Assert(stopped == 0, "Stopping an idle runner raises nothing.");
        runner.StartSequence();
        var wave = runner.CurrentWave;
        Invoke(runner, "Update");
        runner.StopWaves();
        Assert(stopped == 1 && interrupted == wave && !failed && InUse(pool) == 0, "Stop event carries interrupted wave.");
        Assert(((TestPoolProvider)enemy.Provider).PrepareCount == 1, "Provider prepared once per runner.");
    }

    private static void SpawnFailureStopsSequence()
    {
        var enemy = Enemy("Basic");
        var pool = Pool(enemy);
        var runner = Runner(Settings(2, 2, enemy));
        var provider = (WaveSpawnPoints)Get(runner, "spawnLocation");
        On((in WaveEnemySpawnedEvent e) => Set(provider, "points", Array.Empty<Transform>()));
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
        On((in WaveEnemySpawnedEvent e) => { spawnedCount++; runner.StopWaves(); });
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
        Assert(provider.GetSpawnPose(new WaveSpawnRequest(plan, 1)).position == point.position, "Round robin uses second point.");
        var spawned = new List<TestWaveEnemy>();
        On((in WaveEnemySpawnedEvent e) => spawned.Add((TestWaveEnemy)e.Enemy));
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
        ExpectFailure(() => provider.GetSpawnPose(new WaveSpawnRequest(plan, 0)));
        runner.StopWaves();
    }

    private static void NormalizedCurveAxis()
    {
        var basic = Enemy("Basic");
        var advanced = Enemy("Advanced");
        Set(basic, "preference", AnimationCurve.Linear(0f, 1f, 1f, 0f));
        Set(advanced, "preference", AnimationCurve.Linear(0f, 0f, 1f, 1f));
        var settings = Settings(10, 10, basic, advanced);
        Set(settings, "curveAxis", WaveCurveAxis.NormalizedProgress);
        Set(settings, "budget", AnimationCurve.Linear(0f, 2f, 1f, 10f));
        Set(settings, "waveCount", 5);
        Assert(settings.GetCurveTime(1) == 0f && settings.GetCurveTime(3) == 0.5f && settings.GetCurveTime(5) == 1f, "Finite mapping.");
        var generator = new ProceduralWaveGenerator(settings, 7);
        var first = generator.GenerateNextWave();
        Assert(first.Budget == 2 && Count(first, basic) == 2, "Wave 1 evaluates X = 0.");
        generator.GenerateNextWave();
        generator.GenerateNextWave();
        generator.GenerateNextWave();
        var last = generator.GenerateNextWave();
        Assert(last.Budget == 10 && Count(last, advanced) == 10, "Last wave evaluates X = 1.");
        Set(settings, "waveCount", 0);
        Set(settings, "referenceWaveCount", 11);
        Assert(settings.GetCurveTime(11) == 1f && settings.GetCurveTime(21) == 2f, "Unlimited uses the reference wave count.");
        Set(settings, "curveAxis", WaveCurveAxis.WaveNumber);
        Assert(settings.GetCurveTime(7) == 7f, "Default axis is the wave number.");
    }

    private static void SpawnTimingJitter()
    {
        var settings = Settings(30, 30, Enemy("Basic"));
        Set(settings, "spawnDuration", 12f);
        var even = new ProceduralWaveGenerator(settings, 3).GenerateNextWave();
        Set(settings, "spawnTimingJitter", 1f);
        var a = new ProceduralWaveGenerator(settings, 3).GenerateNextWave();
        var b = new ProceduralWaveGenerator(settings, 3).GenerateNextWave();
        Assert(a.Spawns.Select(s => s.Delay).SequenceEqual(b.Spawns.Select(s => s.Delay)), "Jitter is seeded.");
        Assert(a.Spawns.Select(s => s.Enemy).SequenceEqual(even.Spawns.Select(s => s.Enemy)), "Jitter does not change selection.");
        Assert(a.Spawns[0].Delay == 0f && a.Spawns[a.Spawns.Count - 1].Delay == 12f, "Jitter keeps endpoints.");
        bool differs = false;
        for (int i = 1; i < a.Spawns.Count; i++)
        {
            Assert(a.Spawns[i].Delay >= a.Spawns[i - 1].Delay, "Jittered timing is sorted.");
            differs |= Math.Abs(a.Spawns[i].Delay - even.Spawns[i].Delay) > 0.01f;
        }
        Assert(differs, "Jitter changes intervals.");
        Set(settings, "spawnTimingJitter", 2f);
        ExpectFailure(() => new ProceduralWaveGenerator(settings, 1));
    }

    private static void SpawnGroupsRouting()
    {
        var ground = Enemy("Ground");
        var flying = Enemy("Flying");
        Set(ground, "preference", AnimationCurve.Constant(1f, 20f, 1f));
        Set(flying, "preference", AnimationCurve.Constant(1f, 20f, 1f));
        Pool(ground);
        Pool(flying);
        var flyingGroup = Track(ScriptableObject.CreateInstance<WaveSpawnGroup>());
        Set(flying, "spawnGroup", flyingGroup);
        var settings = Settings(20, 20, ground, flying);
        var runner = Runner(settings);
        var groundPoints = (WaveSpawnPoints)Get(runner, "spawnLocation");
        var skyA = Track(new GameObject("SkyA")).transform;
        var skyB = Track(new GameObject("SkyB")).transform;
        skyA.position = new Vector3(0f, 10f, 0f);
        skyB.position = new Vector3(5f, 10f, 0f);
        var flyingPoints = Track(new GameObject("FlyingPoints")).AddComponent<WaveSpawnPoints>();
        Set(flyingPoints, "points", new[] { skyA, skyB });
        var router = Track(new GameObject("Router")).AddComponent<WaveSpawnLocationRouter>();
        Set(router, "defaultLocation", groundPoints);
        var routes = (Array)Get(router, "routes");
        var routeType = routes.GetType().GetElementType();
        var route = Activator.CreateInstance(routeType);
        routeType.GetField("group").SetValue(route, flyingGroup);
        routeType.GetField("location").SetValue(route, flyingPoints);
        var routeArray = Array.CreateInstance(routeType, 1);
        routeArray.SetValue(route, 0);
        Set(router, "routes", routeArray);
        Set(runner, "spawnLocation", router);

        var spawned = new List<TestWaveEnemy>();
        On((in WaveEnemySpawnedEvent e) => spawned.Add((TestWaveEnemy)e.Enemy));
        var plan = new ProceduralWaveGenerator(settings, 11).GenerateNextWave();
        Assert(Count(plan, ground) > 0 && Count(plan, flying) > 1, "Plan mixes both groups.");
        runner.StartWave(plan);
        Invoke(runner, "Update");
        Assert(spawned.Count == plan.Spawns.Count, "All spawned.");
        int flyingSeen = 0;
        foreach (var enemy in spawned)
        {
            if (enemy.Definition == flying)
            {
                var expected = flyingSeen++ % 2 == 0 ? skyA.position : skyB.position;
                Assert(enemy.transform.position == expected, "Flying group uses its own points with per-group round robin.");
            }
            else
            {
                Assert(enemy.transform.position == groundPoints.transform.position, "Ungrouped enemy uses default location.");
            }
        }
        runner.StopWaves();
        Set(router, "defaultLocation", null);
        ExpectFailure(() => router.Resolve(ground));
        Set(router, "defaultLocation", router);
        ExpectFailure(() => router.Resolve(ground));
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
        template.AddComponent<TestWaveEnemy>();
        var pool = Track(ScriptableObject.CreateInstance<ScriptablePool>());
        pool.name = enemy.name + "Pool";
        Set(pool, "pooledObjectPrefab", template);
        Set(pool, "quantityToInstantiate", 1);
        Set(pool, "disableObjectOnCreation", true);
        Set(pool, "disableObjectOnRelease", true);
        var provider = Track(ScriptableObject.CreateInstance<TestPoolProvider>());
        Set(provider, "pool", pool);
        Set(enemy, "provider", provider);
        return pool;
    }

    private static void On<T>(EventCallback<T> callback) where T : struct, IEvent => EvtBus.Subscribe(callback);

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
        EvtBus.ClearAll();
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

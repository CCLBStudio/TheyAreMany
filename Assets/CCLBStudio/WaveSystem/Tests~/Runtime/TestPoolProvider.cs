using CCLBStudio.ScriptablePooling;
using CCLBStudio.WaveSystem;
using UnityEngine;

public sealed class TestPoolProvider : WaveEnemyProvider
{
    [SerializeField] private ScriptablePool pool;

    public int PrepareCount { get; private set; }

    public override void Prepare()
    {
        PrepareCount++;
        pool.Initialize();
    }

    public override IWaveEnemy Provide()
    {
        return pool.RequestObjectAs<TestWaveEnemy>();
    }
}

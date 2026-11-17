using System;
using CCLBStudio.ScriptablePooling;
using CCLBStudio.WaveSystem;
using UnityEngine;

public sealed class TestWaveEnemy : MonoBehaviour, IScriptablePooledObject, IWaveEnemy
{
    public ScriptablePool Pool { get; set; }
    public bool IsSpawned { get; private set; }
    public GameObject GameObject => gameObject;
    public WaveEnemyDefinition Definition { get; private set; }
    public int WaveNumber { get; private set; }

    public event Action<IWaveEnemy> Despawned;

    public void OnObjectCreated()
    {
        IsSpawned = false;
    }

    public void OnObjectRequested()
    {
        IsSpawned = true;
    }

    public void OnObjectReleased()
    {
        IsSpawned = false;
        Definition = null;
        WaveNumber = 0;
        Despawned?.Invoke(this);
    }

    public void OnWaveSpawn(in WaveEnemySpawnContext context)
    {
        Definition = context.Definition;
        WaveNumber = context.WaveNumber;
        transform.SetPositionAndRotation(context.Pose.position, context.Pose.rotation);
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
    }

    public void Despawn()
    {
        Pool.ReleaseObject(this);
    }
}

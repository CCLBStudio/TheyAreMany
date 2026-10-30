using System.Collections.Generic;
using CCLBStudio.EventBus;
using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptablePooling;
using Game.GameOver;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SimpleEnemySpawner : MonoBehaviour, IUpdate, IEventListener<GameOverEvent>
{
    [FormerlySerializedAs("enemyPool")] [SerializeField] private List<ScriptablePool> enemyPools;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] private float timeBetweenSpawns = .5f;

    private float _spawnTimer;
    private bool _shouldSpawn = true;

    private void Start()
    {
        _shouldSpawn = true;
        foreach (var p in enemyPools)
        {
            p.Initialize();
        }
        _spawnTimer = 1f;
        GlobalUpdater.RegisterUpdate(this);
    }

    private void OnEnable()
    {
        EvtBus.Subscribe(this);
    }
    
    private void OnDisable()
    {
        EvtBus.Unsubscribe(this);
    }

    public void Tick()
    {
        _spawnTimer -= Time.deltaTime;

        if (_spawnTimer <= 0f)
        {
            Spawn();
            _spawnTimer = timeBetweenSpawns;
        }
    }

    private void Spawn()
    {
        if (!_shouldSpawn) return;

        var pos = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
        var obj = enemyPools[Random.Range(0, enemyPools.Count)].RequestObjectAs<EnemyFacade>();
        obj.transform.position = pos;
    }

    private void OnDestroy()
    {
        GlobalUpdater.UnregisterUpdate(this);
    }

    public void OnEvent(in GameOverEvent evt)
    {
        _shouldSpawn = false;
    }
}

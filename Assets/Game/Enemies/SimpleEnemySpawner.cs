using System.Collections.Generic;
using CCLBStudio.ScriptablePooling;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class SimpleEnemySpawner : MonoBehaviour
{
    [FormerlySerializedAs("enemyPool")] [SerializeField] private List<ScriptablePool> enemyPools;
    [SerializeField] private List<Transform> spawnPoints;
    [SerializeField] float timeBetweenSpawns = .5f;

    private float _spawnTimer;

    private void Start()
    {
        foreach (var p in enemyPools)
        {
            p.Initialize();
        }
        _spawnTimer = 1f;
    }

    private void Update()
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
        var pos = spawnPoints[Random.Range(0, spawnPoints.Count)].position;
        var obj = enemyPools[Random.Range(0, enemyPools.Count)].RequestObjectAs<EnemyFacade>();
        obj.transform.position = pos;
    }
}

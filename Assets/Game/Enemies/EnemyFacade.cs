using System;
using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptablePooling;
using CCLBStudio.WaveSystem;
using Game.Stats;
using UnityEngine;

public class EnemyFacade : MonoBehaviour, IScriptablePooledObject, IWaveEnemy
{
    public ScriptablePool Pool { get; set; }
    public ScriptableEnemy EnemyData => enemyData;
    public ICharacterStats Stats => _stats;
    public bool IsSpawned { get; private set; }
    public GameObject GameObject => gameObject;

    public event Action<IWaveEnemy> Despawned;

    private IEnemyBehaviour[] _behaviours;
    private ICharacterStats _stats;
    
    [SerializeField] protected ScriptableEnemy enemyData;
    [SerializeField] private EnemyStateMachine stateMachine;

    #region Behaviour Methods

    public void ReleaseSelf()
    {
        Pool.ReleaseObject(this);
    }

    public void OnObjectCreated()
    {
        _stats = GetComponent<ICharacterStats>() ?? new DefaultCharacterStats();
        _behaviours = GetComponentsInChildren<IEnemyBehaviour>();

        foreach (var b in _behaviours)
        {
            b.Facade = this;
            b.OnEnemyCreated();

            if (b is IEnemyState state)
            {
                stateMachine.InitState(state);
            }
        }
    }

    public void OnObjectRequested()
    {
        IsSpawned = true;
        foreach (var b in _behaviours)
        {
            if (b.AutoRegisterToGlobalUpdater())
            {
                GlobalUpdater.RegisterUpdatedObject(b);
            }
            
            b.OnEnemyRequested();
        }
    }

    public void OnObjectReleased()
    {
        foreach (var b in _behaviours)
        {
            if (b.AutoRegisterToGlobalUpdater())
            {
                GlobalUpdater.UnregisterUpdatedObject(b);
            }
            
            b.OnEnemyReleased();
        }

        IsSpawned = false;
        Despawned?.Invoke(this);
    }

    #endregion

    #region Wave Methods

    public void OnWaveSpawn(in WaveEnemySpawnContext context)
    {
        transform.SetPositionAndRotation(context.Pose.position, context.Pose.rotation);
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    public void Despawn()
    {
        ReleaseSelf();
    }

    #endregion
}

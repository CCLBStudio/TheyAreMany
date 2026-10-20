using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptablePooling;
using Game.Stats;
using UnityEngine;

public class EnemyFacade : MonoBehaviour, IScriptablePooledObject
{
    public ScriptablePool Pool { get; set; }
    public ScriptableEnemy EnemyData => enemyData;
    public ICharacterStats Stats => _stats;

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
    }

    #endregion
}

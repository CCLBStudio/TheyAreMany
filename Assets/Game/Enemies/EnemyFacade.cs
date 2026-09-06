using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptablePooling;
using UnityEngine;
using ZLinq;

public class EnemyFacade : MonoBehaviour, IScriptablePooledObject
{
    public ScriptablePool Pool { get; set; }
    public ScriptableEnemy EnemyData => enemyData;

    private IEnemyBehaviour[] _behaviours;
    
    [SerializeField] protected ScriptableEnemy enemyData;
    [SerializeField] private EnemyStateMachine stateMachine;

    #region Behaviour Methods

    public void ReleaseSelf()
    {
        Pool.ReleaseObject(this);
    }

    public void OnObjectCreated()
    {
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
        foreach (var b in _behaviours.AsValueEnumerable().Where(x => x.AutoRegisterToGlobalUpdater()))
        {
            GlobalUpdater.RegisterUpdatedObject(b);
            b.OnEnemyRequested();
        }
    }

    public void OnObjectReleased()
    {
        foreach (var b in _behaviours.AsValueEnumerable().Where(x => x.AutoRegisterToGlobalUpdater()))
        {
            GlobalUpdater.UnregisterUpdatedObject(b);
            b.OnEnemyReleased();
        }
    }

    #endregion
}

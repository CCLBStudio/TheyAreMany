using UnityEngine;

public class EnemyDeathState : MonoBehaviour, IEnemyBehaviour, IEnemyState
{
    public EnemyFacade Facade { get; set; }

    public EnemyStateMachine StateMachine { get; set; }
    public void EnterState()
    {
    }

    public void UpdateState()
    {
    }

    public void ExitState()
    {
    }

    public EnemyStateId GetStateId()
    {
        return EnemyStateId.Die;
    }

    public void OnEnemyCreated()
    {
    }

    public void OnEnemyRequested()
    {
    }

    public void OnEnemyReleased()
    {
    }
}

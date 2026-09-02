using UnityEngine;

public class EnemyStateMachine : MonoBehaviour, IEnemyBehaviour
{
    public EnemyFacade Facade { get; set; }

    public IEnemyState IdleState { get; private set; }
    public IEnemyState ChaseState { get; private set; }
    public IEnemyState AttackState { get; private set; }
    public IEnemyState DieState { get; private set; }
    
    private IEnemyState _currentState;
    
    public void TransitionToState(IEnemyState newState)
    {
        _currentState?.ExitState();
        _currentState = newState;
        _currentState?.EnterState();
    }

    public void InitState(IEnemyState state)
    {
        state.StateMachine = this;

        switch (state.GetStateId())
        {
            case EnemyStateId.Idle:
                IdleState = state;
                break;
            
            case EnemyStateId.Chase:
                ChaseState = state;
                break;
            
            case EnemyStateId.Attack:
                AttackState = state;
                break;
            
            case EnemyStateId.Die:
                DieState = state;
                break;
        }
    }
    
    public void OnEnemyCreated()
    {

    }

    public void OnEnemyRequested()
    {
        TransitionToState(ChaseState);
    }

    public void OnEnemyReleased()
    {
    }

    public void OnFixedUpdated()
    {
        _currentState.UpdateState();
    }

    public void ToDeathState()
    {
        TransitionToState(DieState);
    }
}

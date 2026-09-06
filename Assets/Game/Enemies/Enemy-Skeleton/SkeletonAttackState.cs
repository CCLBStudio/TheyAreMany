using UnityEngine;

public class SkeletonAttackState : EnemyAttackState
{
    [SerializeField] private Collider weaponCollider;
    
    private Vector2 _desiredPosition;
    
    private void ComputeDesiredPosition()
    {
        _desiredPosition = Target.position;
        _desiredPosition.y = rb.position.y;
    }
    
    private float ComputeTargetDistance()
    {
        return Vector2.Distance(rb.position, _desiredPosition);
    }

    public override void EnterState()
    {
        ComputeDesiredPosition();
    }

    public override void UpdateState()
    {
        if (ComputeTargetDistance() > EnemyData.AttackRange)
        {
            StateMachine.TransitionToState(StateMachine.ChaseState);
            return;
        }
        
        ComputeDesiredPosition();

        if (attackTimer <= 0f && Vector2.Distance(rb.position, Target.position) < EnemyData.AttackRange)
        {
            LaunchAttack();
        }
    }
}

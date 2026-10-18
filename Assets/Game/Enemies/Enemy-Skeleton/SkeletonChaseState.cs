using UnityEngine;

public class SkeletonChaseState : EnemyChaseState
{
    private Vector2 _desiredPosition;
    
    protected override void Move()
    {
        if (!Target)
        {
            return;
        }
        
        if (ComputeTargetDistance() < EnemyData.AttackRange)
        {
            StateMachine.TransitionToState(StateMachine.AttackState);
            return;
        }
        
        ComputeDesiredPosition();

        Vector2 dir = (_desiredPosition - rb.position).normalized;
        rb.MovePosition(rb.position + dir * (speed * Time.fixedDeltaTime));
    }

    private void ComputeDesiredPosition()
    {
        _desiredPosition = Target.position;
        _desiredPosition.y = rb.position.y;
    }

    private float ComputeTargetDistance()
    {
        return Vector2.Distance(rb.position, Target.position);
    }
}

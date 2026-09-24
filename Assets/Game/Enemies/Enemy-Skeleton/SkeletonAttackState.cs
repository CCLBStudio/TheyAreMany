using UnityEngine;

public class SkeletonAttackState : EnemyAttackState
{
    [SerializeField] private Collider attackCollider;
    
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

    public void OnAttackBeginDangerous()
    {
        EnableDamageCollider(true);
    }
    
    public void OnAttackEndDangerous()
    {
        EnableDamageCollider(false);
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
    
    private void EnableDamageCollider(bool enable)
    {
        if (!attackCollider)
        {
            Debug.LogError("Attack collider is not assigned.");
            return;
        }
        attackCollider.enabled = enable;
    }
}

using System.Collections.Generic;
using CCLBStudio.Extensions;
using Game.Enemies;
using UnityEngine;

public class SkeletonAttackState : EnemyAttackState
{
    [SerializeField] private Collider attackCollider;
    
    private readonly List<IEnemyTarget> _collidingTargets = new();

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.collider.TryGetComponentOnRoot(out IEnemyTarget target))
        {
            return;
        }
        
        _collidingTargets.Add(target);
    }
    
    private void OnCollisionExit2D(Collision2D other)
    {
        if (!other.collider.TryGetComponentOnRoot(out IEnemyTarget target))
        {
            return;
        }
        
        _collidingTargets.Remove(target);
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
    }

    public override void UpdateState()
    {
        if (_collidingTargets.Count <= 0)
        {
            StateMachine.TransitionToState(StateMachine.ChaseState);
            return;
        }
        
        if (attackTimer <= 0f)
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

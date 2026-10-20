using System;
using CCLBStudio.GlobalUpdater;
using Game.Damage;
using Game.Enemies;
using UnityEngine;

public class EnemyAttackState : MonoBehaviour, IEnemyBehaviour, IFixedUpdate, IEnemyState
{
    public virtual EnemyFacade Facade { get; set; }
    public virtual EnemyStateMachine StateMachine { get; set; }
    protected Transform Target => targetSelector?.Target;
    protected ScriptableEnemy EnemyData => Facade?.EnemyData;

    [SerializeField] protected EnemyTargetSelector targetSelector;
    [SerializeField] private EnemyAnimator animPlayer;
    [SerializeField] protected Rigidbody2D rb;

    protected float attackTimer;
    protected Quaternion baseRot;

    protected void LaunchAttack()
    {
        animPlayer.PlayAttackAnimation();
        attackTimer = 1f / Calculator.ComputeAttackSpeed(Facade.EnemyData.AttackSpeed, Facade.Stats.AttackSpeed.Value);
    }
    
    protected void OrientTowardsTarget()
    {
        if (!Target)
        {
            return;
        }

        float targetPos = Target.position.x;
        float selfPos = rb.position.x;

        if (targetPos > selfPos)
        {
            transform.rotation = baseRot * Quaternion.Euler(0f, 180f, 0f);
        }
        else
        {
            transform.rotation = baseRot;
        }
    }

    #region Behaviour Methods

    public virtual void OnEnemyCreated()
    {
        baseRot = transform.rotation;
    }

    public virtual void OnEnemyRequested()
    {
        attackTimer = 0f;
    }

    public virtual void OnEnemyReleased()
    {
    }

    public void FixedTick()
    {
        if (attackTimer > 0f)
        {
            attackTimer -= Time.fixedDeltaTime;
        }
    }

    #endregion

    #region State Machine Methods

    public virtual void EnterState()
    {
    }

    public virtual void UpdateState()
    {
        
    }

    public virtual void ExitState()
    {
    }

    public virtual EnemyStateId GetStateId()
    {
        return EnemyStateId.Attack;
    }

    #endregion

    #region Editor
    #if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying || !rb || !EnemyData)
        {
            return;
        }
        
        Gizmos.DrawWireSphere(rb.position, EnemyData.AttackRange);
    }

#endif
    #endregion
}

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class EnemyChaseState : MonoBehaviour, IEnemyBehaviour, IEnemyState
{
    public EnemyFacade Facade { get; set; }
    public EnemyStateMachine StateMachine { get; set; }
    public bool CanMove { get; set; } = true;
    protected Transform Target => targetSelector.Target;
    protected ScriptableEnemy EnemyData => Facade.EnemyData;

    [SerializeField] protected EnemyTargetSelector targetSelector;
    [SerializeField] protected EnemyAnimator animPlayer;
    [SerializeField] protected Rigidbody2D rb;
    [SerializeField] protected Vector2 minMaxSpeed = Vector2.one;
    [SerializeField] protected float heightDivider = 2f;

    [Header("Events")]
    [SerializeField] private UnityEvent startMoving;
    [SerializeField] private UnityEvent stopMoving;

    protected float speed;
    protected Vector2 direction;
    protected Quaternion baseRot;

    protected virtual void Move()
    {
        direction = targetSelector.Target.position - transform.position;
        direction.y /= heightDivider;
        
        rb.AddForce(direction.normalized * speed);
    }

    protected void OrientTowardsTarget()
    {
        if (Target == null)
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
    public virtual void OnEnemyCreated()
    {
        speed = Random.Range(minMaxSpeed.x, minMaxSpeed.y);
        baseRot = transform.rotation;
    }

    public virtual void OnEnemyRequested()
    {
        CanMove = true;
    }

    public virtual void OnEnemyReleased()
    {
    }
    
    #region State Machine Methods

    public virtual void EnterState()
    {
        CanMove = true;
        animPlayer.PlayRunAnimation();
        startMoving?.Invoke();
    }

    public virtual void UpdateState()
    {
        OrientTowardsTarget();
        Move();
    }

    public virtual void ExitState()
    {
        CanMove = false;
        stopMoving?.Invoke();
    }

    public virtual EnemyStateId GetStateId()
    {
        return EnemyStateId.Chase;
    }

    #endregion
}

using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptablePooling;
using Game.Damage;
using Game.ModularAbility;
using Game.Stats;
using MoreMountains.Feedbacks;
using PrimeTween;
using UnityEngine;

public class PooledRocket : MonoBehaviour, IScriptablePooledObject, IDamageSource, IFixedUpdate
{
    public ScriptablePool Pool { get; set; }

    [SerializeField] private Collider2D rocketCollider;
    [SerializeField] private MMF_Player feedback;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask collisionMask;
    
    private float _currentSpeed;
    private Tween _accelerationTween;
    private bool _isAlive;
    private float _lifetime;
    private int _baseDamage;
    private float _explosionRange;
    private ScriptablePool _explosionPool;
    private ICharacterStats _ownerStats;

    public void Initialize(RocketContext context)
    {
        _baseDamage = context.BaseDamage;
        _lifetime = context.Lifetime;
        _explosionPool = context.ExplosionPool;
        _explosionRange = context.ExplosionRange;
        _ownerStats = context.OwnerStats ?? new DefaultCharacterStats();
        _accelerationTween = Tween.Custom(0f, context.TravelSpeed, context.AccelerationTime, f => _currentSpeed = f, context.AccelerationEase);
        
        rocketCollider.enabled = true;
    }

    public void FixedTick()
    {
        _lifetime -= Time.fixedDeltaTime;
        if (_lifetime <= 0f)
        {
            Pool.ReleaseObject(this);
            return;
        }
        
        Vector2 right = transform.right;
        rb.MovePosition(rb.position + right * (_currentSpeed * Time.fixedDeltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isAlive)
        {
            return;
        }
        _isAlive = false;

        if (_accelerationTween.isAlive)
        {
            _accelerationTween.Stop();
        }
        
        PlayExplosionParticles();
        ApplyDamages();
        feedback?.PlayFeedbacks();
        Pool.ReleaseObject(this);
    }

    private void PlayExplosionParticles()
    {
        var effect = _explosionPool.RequestObjectAs<PooledParticleSystem>();
        effect.transform.position = transform.position;
        effect.Play();
    }

    private void ApplyDamages()
    {
        Collider2D[] inRange = Physics2D.OverlapCircleAll(transform.position, _explosionRange, collisionMask);
        foreach (var col in inRange)
        {
            var damageTargets = col.gameObject.GetComponents<IDamageTarget>();
            foreach (var target in damageTargets)
            {
                var ctx = ComputeDamageContextForTarget(target);
                target.ReceiveDamages(ctx);
            }
        }
    }
    
    private DamageContext ComputeDamageContextForTarget(IDamageTarget target)
    {
        if (target is IResistanceProvider resProvider)
        {
            var res = resProvider.GetResistances(GetDamageType());
            return new DamageContext(this, Calculator.ComputeDamage(new DamageCalculationContext
            {
                BaseDamage = _baseDamage,
                AttackerPower = _ownerStats.Power.Value,
                AttackerFlatDamage = _ownerStats.FlatDamage.Value,
                AttackerFinalDamagePercentage = _ownerStats.FinalDamagePercentage.Value,
                TargetFlatResistance = res.FlatResistance.Value,
                TargetPercentageResistance = res.PercentageResistance.Value
            }));
        }
            
        return new DamageContext(this, Calculator.ComputeDamage(new DamageCalculationContext
        {
            BaseDamage = _baseDamage,
            AttackerPower = _ownerStats.Power.Value,
            AttackerFlatDamage = _ownerStats.FlatDamage.Value,
            AttackerFinalDamagePercentage = _ownerStats.FinalDamagePercentage.Value,
            TargetFlatResistance = 0,
            TargetPercentageResistance = 0
        }));
    }

    public void OnObjectCreated()
    {
        rocketCollider.enabled = false;
        _isAlive = false;
    }

    public void OnObjectRequested()
    {
        GlobalUpdater.RegisterFixedUpdate(this);
        _isAlive = true;
    }
    
    public void OnObjectReleased()
    {
        GlobalUpdater.UnregisterFixedUpdate(this);
        rocketCollider.enabled = false;
    }

    #region Damageable Methods

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public DamageType GetDamageType()
    {
        return DamageType.Explosive;
    }

    #endregion

#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (_explosionRange <= 0f)
        {
            return;
        }
        Gizmos.DrawWireSphere(transform.position, _explosionRange);
    }

#endif
}
using CCLBStudio.GlobalUpdater;
using Game.Damage;
using Game.Stats;
using MoreMountains.Feedbacks;
using PrimeTween;
using UnityEngine;

public class PooledRocket : PooledAbilityObject<ScriptableRocketAbility>, IDamageSource, IFixedUpdate
{
    [SerializeField] private Collider2D rocketCollider;
    [SerializeField] private MMF_Player feedback;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask collisionMask;
    
    private float _currentSpeed;
    private Tween _accelerationTween;
    private bool _isAlive;
    private float _lifetime;
    private ScriptableRocketAbility _scriptableAbility;
    private ICharacterStats _defaultStats = new DefaultCharacterStats();

    public override void Initialize(ScriptableRocketAbility scriptableAbility)
    {
        _scriptableAbility = scriptableAbility;
        
        _lifetime = _scriptableAbility.Lifetime;
        float maxSpeed = _scriptableAbility.TravelSpeed;
        _accelerationTween = Tween.Custom(0f, maxSpeed, _scriptableAbility.AccelerationTime, f => _currentSpeed = f, _scriptableAbility.AccelerationEase);
        
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
        var effect = _scriptableAbility.ExplosionPool.RequestObjectAs<PooledParticleSystem>();
        Vector3 abilityPos = transform.position;
        effect.transform.position = abilityPos;
        effect.Play();
    }

    private void ApplyDamages()
    {
        Collider2D[] inRange = Physics2D.OverlapCircleAll(transform.position, _scriptableAbility.ExplosionRange, collisionMask);
        foreach (var col in inRange)
        {
            var damageTargets = col.gameObject.GetComponents<IDamageTarget>();
            foreach (var d in damageTargets)
            {
                var ctx = new DamageContext(this, Calculator.ComputeDamage(new DamageCalculationContext
                {
                    BaseDamage = _scriptableAbility.Strength,
                    AttackerPower = _defaultStats.Power.Value,
                    AttackerFlatDamage = _defaultStats.FlatDamage.Value,
                    AttackerFinalDamagePercentage = _defaultStats.FinalDamagePercentage.Value,
                    TargetFlatResistance = d.GetResistances(GetDamageType()).FlatResistance.Value,
                    TargetPercentageResistance = d.GetResistances(GetDamageType()).PercentageResistance.Value
                }));
                d.ReceiveDamages(ctx);
            }
        }
    }

    public override void OnObjectCreated()
    {
        rocketCollider.enabled = false;
        _isAlive = false;
    }

    public override void OnObjectRequested()
    {
        GlobalUpdater.RegisterFixedUpdate(this);
        _isAlive = true;
    }
    
    public override void OnObjectReleased()
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
        if (!_scriptableAbility)
        {
            return;
        }
        Gizmos.DrawWireSphere(transform.position, _scriptableAbility.ExplosionRange);
    }

#endif
}

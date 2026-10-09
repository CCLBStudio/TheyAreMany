using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptablePooling;
using Game.Damage;
using Game.ModularWeapon;
using Game.Stats;
using UnityEngine;

public class RuntimeBullet : MonoBehaviour, IScriptablePooledObject, IDamageSource, IFixedUpdate
{
    public ScriptablePool Pool { get; set; }
    public Vector2 Direction { get; set; }

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D bulletCollider;

    private bool _isAlive;
    private bool _isInit;
    private float _currentLifetime;
    private int _baseDamage;
    private float _speed;
    private ICharacterStats _ownerStats;

    public void FixedTick()
    {
        if(!_isAlive || !_isInit)
        {
            return;
        }

        _currentLifetime -= Time.fixedDeltaTime;
        if (_currentLifetime <= 0f)
        {
            Pool.ReleaseObject(this);
            return;
        }

        rb.MovePosition(rb.position + Direction * (_speed * Time.fixedDeltaTime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isAlive)
        {
            return;
        }

        _isAlive = false;
        
        var interactors = other.gameObject.GetComponents<IDamageTarget>();
        if (interactors.Length <= 0)
        {
            Pool.ReleaseObject(this);
            return;
        }

        var hitPoint = other.ClosestPoint(transform.position);
        
        foreach (var i in interactors)
        {
            var ctx = new DamageContext(this, ComputeDamage(i.GetResistances(GetDamageType())));
            i.ReceiveDamages(ctx, hitPoint);
        }
        
        Pool.ReleaseObject(this);
    }
    
    private ComputedDamage ComputeDamage(ResistanceDescriptor res)
    {
        return Calculator.ComputeDamage(new DamageCalculationContext
        {
            BaseDamage = _baseDamage,
            AttackerPower = _ownerStats.Power.Value,
            AttackerFlatDamage = _ownerStats.FlatDamage.Value,
            AttackerFinalDamagePercentage = _ownerStats.FinalDamagePercentage.Value,
            TargetFlatResistance = res.FlatResistance.Value,
            TargetPercentageResistance = res.PercentageResistance.Value
        });
    }
    
    public void Initialize(IRuntimeBulletContext context)
    {
        _baseDamage = context.BaseDamage;
        _ownerStats = context.OwnerStats;
        _speed = context.Speed + context.OwnerStats.BulletSpeed.Value;
        _currentLifetime = context.LifeTime;
        _isInit = true;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public DamageType GetDamageType()
    {
        return DamageType.Piercing;
    }

    public void OnObjectCreated()
    {
        _isAlive = false;
        _isInit = false;
    }

    public void OnObjectReleased()
    {
        _isAlive = false;
        _isInit = false;
        GlobalUpdater.UnregisterFixedUpdate(this);
    }

    public void OnObjectRequested()
    {
        _isAlive = true;
        GlobalUpdater.RegisterFixedUpdate(this);
    }
}

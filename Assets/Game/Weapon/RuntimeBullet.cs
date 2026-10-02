using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptablePooling;
using Game.ModularWeapon;
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
    private IRuntimeBulletStat _bulletStat;

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

        rb.MovePosition(rb.position + Direction * (_bulletStat.GetSpeed() * Time.fixedDeltaTime));
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
        var ctx = new DamageContext(this, _bulletStat.GetDamage());
        
        foreach (var i in interactors)
        {
            i.ReceiveDamages(ctx, hitPoint);
        }
        
        Pool.ReleaseObject(this);
    }
    
    public void Initialize(IRuntimeBulletStat stat)
    {
        _bulletStat = stat;
        _currentLifetime = stat.GetLifetime();
        _isInit = true;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public DamageType GetDamageType()
    {
        return DamageType.Bullet;
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

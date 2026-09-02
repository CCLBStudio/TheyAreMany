using CCLBStudio.ScriptablePooling;
using UnityEngine;

public class RuntimeBullet : MonoBehaviour, IScriptablePooledObject, IDamageSource
{
    public ScriptablePool Pool { get; set; }
    public Vector2 Direction { get; set; }

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D bulletCollider;

    private bool _isAlive;
    private bool _isInit;
    private float _currentLifetime;
    private ScriptableWeapon _currentWeapon;

    void FixedUpdate()
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

        rb.MovePosition(rb.position + Direction * (_currentWeapon.BulletSpeed * Time.fixedDeltaTime));
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
            var effect = _currentWeapon.GroundImpactPool.RequestObjectAs<PooledParticleSystem>();
            effect.transform.position = other.ClosestPoint(transform.position);
            effect.Play();
            Pool.ReleaseObject(this);
            return;
        }

        foreach (var i in interactors)
        {
            i.ReceiveDamages(this);
        }
        
        Pool.ReleaseObject(this);
    }
    
    public void Initialize(ScriptableWeapon weapon)
    {
        _currentWeapon = weapon;
        _currentLifetime = weapon.BulletLifetime;
        _isInit = true;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public DamageType GetDamageType()
    {
        return DamageType.Impact;
    }

    public float GetDamages()
    {
        return _currentWeapon.Damages;
    }

    public float GetKnockbackForce()
    {
        return _currentWeapon.DamageableKnockbackForce;
    }

    public Collider2D GetCollider()
    {
        return bulletCollider;
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
    }

    public void OnObjectRequested()
    {
        _isAlive = true;
    }
}

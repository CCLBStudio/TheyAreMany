using UnityEngine;

public interface IDamageSource
{
    public Vector3 GetPosition();
    public DamageType GetDamageType();
}

public interface IDamageContext
{
    public IDamageSource Source();
    public float Amount();
}

public readonly struct DamageContext : IDamageContext
{
    private readonly IDamageSource _damageSource;
    private readonly float _damageAmount;

    public DamageContext(IDamageSource damageSource, float damageAmount)
    {
        _damageSource = damageSource;
        _damageAmount = damageAmount;
    }

    public IDamageSource Source()
    {
        return _damageSource;
    }

    public float Amount()
    {
        return _damageAmount;
    }
}

public class DebugDamageSource : IDamageSource
{
    private readonly Vector3 _position;
    private readonly DamageType _damageType;

    public DebugDamageSource(Vector3 position, DamageType damageType)
    {
        _position = position;
        _damageType = damageType;
    }

    public Vector3 GetPosition()
    {
        return _position;
    }

    public DamageType GetDamageType()
    {
        return _damageType;
    }
}

using UnityEngine;

public interface IDamageSource
{
    public Vector3 GetPosition();
    public DamageType GetDamageType();
    public float GetDamages();
}

public class DebugDamageSource : IDamageSource
{
    private Vector3 _position;
    private DamageType _damageType;
    private float _damages;

    public DebugDamageSource(Vector3 position, DamageType damageType, float damages)
    {
        _position = position;
        _damageType = damageType;
        _damages = damages;
    }

    public Vector3 GetPosition()
    {
        return _position;
    }

    public DamageType GetDamageType()
    {
        return _damageType;
    }

    public float GetDamages()
    {
        return _damages;
    }
}

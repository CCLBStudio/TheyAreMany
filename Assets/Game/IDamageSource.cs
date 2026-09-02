using UnityEngine;

public interface IDamageSource
{
    public Vector3 GetPosition();
    public DamageType GetDamageType();
    public float GetDamages();
}

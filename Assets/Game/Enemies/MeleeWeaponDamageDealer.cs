using UnityEngine;

public class MeleeWeaponDamageDealer : MonoBehaviour, IDamageSource
{
    private Vector3? _hitPoint;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageTarget t))
        {
            _hitPoint = other.ClosestPoint(transform.position);
            t.ReceiveDamages(this);
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3? GetHitPoint()
    {
        return _hitPoint;
    }

    public DamageType GetDamageType()
    {
        return DamageType.Slash;
    }

    public float GetDamages()
    {
        return 10f;
    }
}

using UnityEngine;

public class MeleeWeaponDamageDealer : MonoBehaviour, IDamageSource
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageTarget t))
        {
            var hitPoint = other.ClosestPoint(transform.position);
            var ctx = new DamageContext(this, 10f);
            t.ReceiveDamages(ctx, hitPoint);
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public DamageType GetDamageType()
    {
        return DamageType.Slash;
    }
}

using Game.Damage;
using UnityEngine;

public class MeleeWeaponDamageDealer : MonoBehaviour, IDamageSource
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamageTarget t))
        {
            var hitPoint = other.ClosestPoint(transform.position);
            var ctx = new DamageContext(this, new ComputedDamage
            {
                TrueDamage = 10,
                ReducedDamage = 10,
                ReducedAmount = 0,
                FinalDamage = 10,
                IsCritical = false
            });
            t.ReceiveDamages(ctx, hitPoint);
        }
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public DamageType GetDamageType()
    {
        return DamageType.Slashing;
    }
}

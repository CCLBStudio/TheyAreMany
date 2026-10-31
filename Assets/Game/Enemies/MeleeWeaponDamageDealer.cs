using CCLBStudio.Extensions;
using Game.Damage;
using Game.Stats;
using UnityEngine;

public class MeleeWeaponDamageDealer : MonoBehaviour, IDamageSource
{
    [SerializeField] private RangeInt baseDamage = new(7, 10);
    private ICharacterStats _stats;

    private void Awake()
    {
        _stats = this.TryGetComponentOnRoot(out ICharacterStats stats) ? stats : new DefaultCharacterStats();
    }

    private void OnTriggerEnter(Collider other)
    {
        var targets = other.GetComponents<IDamageTarget>();
        if (targets.Length == 0)
        {
            return;
        }
        
        var hitPoint = other.ClosestPoint(transform.position);
        foreach (var t in targets)
        {
            var ctx = DamageHelper.ComputeDamageContext(this, baseDamage.GetRandom(), _stats, t);
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

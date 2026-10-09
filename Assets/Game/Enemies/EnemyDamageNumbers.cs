using CCLBStudio.Attributes;
using DamageNumbersPro;
using Game.Damage;
using Game.Stats;
using UnityEngine;

public class EnemyDamageNumbers : MonoBehaviour, IDamageTarget
{
    [SerializeField] DamageNumber damageNumberPrefab;
    [NullInfo("Will use self position")]
    [SerializeField] private Transform damageNumberSpawnPoint;
    
    private Transform _target;
    private ICharacterStats _stats = new DefaultCharacterStats();

    private void Start()
    {
        _target = damageNumberSpawnPoint ? damageNumberSpawnPoint : transform;
    }

    public ResistanceDescriptor GetResistances(DamageType damageType)
    {
        return _stats.GetResistances(damageType);
    }

    public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint)
    {
        damageNumberPrefab.Spawn(hitPoint ?? _target.position, damageContext.Damage.FinalDamage);
    }
}

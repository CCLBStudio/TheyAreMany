using CCLBStudio.Attributes;
using DamageNumbersPro;
using UnityEngine;

public class EnemyDamageNumbers : MonoBehaviour, IDamageTarget
{
    [SerializeField] DamageNumber damageNumberPrefab;
    [NullInfo("Will use self position")]
    [SerializeField] private Transform damageNumberSpawnPoint;
    
    private Transform _target;

    private void Start()
    {
        _target = damageNumberSpawnPoint ? damageNumberSpawnPoint : transform;
    }

    public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint)
    {
        damageNumberPrefab.Spawn(hitPoint ?? _target.position, damageContext.Amount());
    }
}

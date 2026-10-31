using CCLBStudio.Attributes;
using DamageNumbersPro;
using UnityEngine;

namespace Game.Damage
{
    public class DamageNumberSpawner : MonoBehaviour
    {
        [SerializeField] DamageNumber damageNumberPrefab;
        [NullInfo("Will use self position")]
        [SerializeField] private Transform damageNumberSpawnPoint;
    
        private Transform _target;
        private IHealth _health;

        private void Start()
        {
            _target = damageNumberSpawnPoint ? damageNumberSpawnPoint : transform;
        }

        public void Spawn(IDamageContext damageContext, Vector3? hitPoint)
        {
            damageNumberPrefab.Spawn(hitPoint ?? _target.position, damageContext.Damage.FinalDamage);
        }
    }
}

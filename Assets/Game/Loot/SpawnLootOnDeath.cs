using CCLBStudio.DependencyInjection;
using CCLBStudio.Loot;
using Game.Damage;
using UnityEngine;

namespace Game.Loot
{
    public class SpawnLootOnDeath : InjectedMonoBehaviour
    {
        [SerializeField] private LootTable lootTable;

        protected override void Start()
        {
            base.Start();
            
            if(!TryGetComponent(out IHealth health))
            {
                Debug.LogError($"IHealth component not found on root {transform.root.name}.");
                return;
            }
            
            health.OnDeath -= SpawnLoot;
            health.OnDeath += SpawnLoot;
        }
        
        private void SpawnLoot(IDamageContext damageContext)
        {
            if(!lootTable)
            {
                Debug.LogError("LootTable is not assigned.");
                return;
            }
            
            if(!lootTable.TryGetLoot(out var loot) || loot == null)
            {
                return;
            }
            
            Vector3 pos = transform.position;
            pos.y = 0f;
            loot.Spawn(pos, Quaternion.identity);
        }
    }
}
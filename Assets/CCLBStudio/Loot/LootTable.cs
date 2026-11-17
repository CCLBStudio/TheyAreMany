using CCLBStudio.DependencyInjection;
using UnityEngine;

namespace CCLBStudio.Loot
{
    [CreateAssetMenu(fileName = "New Loot Table", menuName = "CCLB Studio/Loot/Table")]
    public partial class LootTable : ScriptableObject
    {
        public WeightedEntry[] Entries => entries;
        
        [Tooltip("The chance that this loot table will drop any loot when TryGetLoot is called. 1 = 100% chance, 0 = 0% chance.")]
        [SerializeField][Range(0f, 1f)] private float chanceToDrop = 1f;
        [SerializeField] private WeightedEntry[] entries;
        
        [Inject] private LootService lootService;

        public bool TryGetLoot(out ILoot loot)
        {
            loot = null;
            if(chanceToDrop <= 0f)
            {
                return false;
            }

            if (chanceToDrop >= 1f || Random.value <= chanceToDrop)
            {
                loot = lootService.Pick(this);
                return true;
            }

            return false;
        }
    }
}
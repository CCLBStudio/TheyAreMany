using Game.Damage;
using Game.Stats;
using UnityEngine;

namespace Game.Loot
{
    public class ResistanceRuntimeLoot : RuntimeLoot<ICharacterStats>
    {
        [SerializeField] private DamageType resistanceType;
        [SerializeField][Min(0)] private int flatAmount = 1;
        [SerializeField][Min(0)] private int percentageAmount = 1;
        
        protected override void Consume(ICharacterStats component)
        {
            var res = component.GetResistances(resistanceType);
            if (flatAmount != 0)
            {
                res.FlatResistance.AddModifier(new StatModifier<int>(flatAmount, component));
            }
            if (percentageAmount != 0)
            {
                res.PercentageResistance.AddModifier(new StatModifier<int>(percentageAmount, component));
            }
        }
    }
}
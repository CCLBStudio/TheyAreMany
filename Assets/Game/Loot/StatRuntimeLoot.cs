using System;
using Game.Stats;
using UnityEngine;

namespace Game.Loot
{
    public class StatRuntimeLoot : RuntimeLoot<ICharacterStats>
    {
        [SerializeField] private LootStatType lootStatType;
        [SerializeField] private float amount;
        
        private enum LootStatType {
            FlatDamage,
            Power,
            CritChance,
            CritDamage,
            FinalDamagePercentage,
            AttackSpeed,
            ProjectileSpeed,
            Prismatic
        }
        
        protected override void Consume(ICharacterStats component)
        {
            switch (lootStatType)
            {
                case LootStatType.FlatDamage:
                    component.FlatDamage.AddModifier(new StatModifier<int>((int)amount, component));
                    break;
                
                case LootStatType.Power:
                    component.Power.AddModifier(new StatModifier<int>((int)amount, component));
                    break;
                
                case LootStatType.CritChance:
                    component.CritChance.AddModifier(new StatModifier<int>((int)amount, component));
                    break;
                
                case LootStatType.CritDamage:
                    component.CritFlatDamage.AddModifier(new StatModifier<int>((int)amount, component));
                    break;
                
                case LootStatType.FinalDamagePercentage:
                    component.FinalDamagePercentage.AddModifier(new StatModifier<float>(amount, component));
                    break;
                
                case LootStatType.AttackSpeed:
                    component.AttackSpeed.AddModifier(new StatModifier<float>(amount, component));
                    break;
                
                case LootStatType.ProjectileSpeed:
                    component.BulletSpeed.AddModifier(new StatModifier<float>(amount, component));
                    break;
                
                case LootStatType.Prismatic:
                    component.FlatDamage.AddModifier(new StatModifier<int>((int)amount, component));
                    component.Power.AddModifier(new StatModifier<int>((int)amount, component));
                    component.CritChance.AddModifier(new StatModifier<int>((int)amount, component));
                    component.CritFlatDamage.AddModifier(new StatModifier<int>((int)amount, component));
                    component.FinalDamagePercentage.AddModifier(new StatModifier<float>(amount, component));
                    component.AttackSpeed.AddModifier(new StatModifier<float>(amount, component));
                    component.BulletSpeed.AddModifier(new StatModifier<float>(amount, component));
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
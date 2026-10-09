using System;
using System.Collections.Generic;
using Game.Damage;
using UnityEngine;

namespace Game.Stats
{
    public interface ICharacterStats
    {
        IStat<int> FlatDamage { get; }
        IStat<int> Power { get; }
        IStat<int> CritChance { get; }
        IStat<int> CritFlatDamage { get; }
        IStat<float> FinalDamagePercentage { get; }
        IStat<float> AttackSpeed { get; }
        IStat<float> BulletSpeed { get; }
        ResistanceDescriptor GetResistances(DamageType damageType);
    }
    
    public class CharacterStats : MonoBehaviour, ICharacterStats
    {
        public IStat<float> AttackSpeed => attackSpeed;
        public IStat<int> FlatDamage => damage;
        public IStat<int> Power => power;
        public IStat<int> CritChance => critChance;
        public IStat<int> CritFlatDamage => critDamage;
        public IStat<float> FinalDamagePercentage => finalDamagePercentage;
        public IStat<float> BulletSpeed => bulletSpeed;
        
        [SerializeReference] private IStat<int> damage = new IntStat();
        [SerializeReference] private IStat<int> power = new IntStat();
        [SerializeReference] private IStat<int> critChance = new IntStat();
        [SerializeReference] private IStat<int> critDamage = new IntStat();
        [SerializeReference] private IStat<float> finalDamagePercentage = new FloatStat();
        [SerializeReference] private IStat<float> attackSpeed = new FloatStat();
        [SerializeReference] private IStat<float> bulletSpeed = new FloatStat();
        [SerializeField] private Dictionary<DamageType, ResistanceDescriptor> resistances = new();
        
        public ResistanceDescriptor GetResistances(DamageType damageType)
        {
            return resistances.TryGetValue(damageType, out var descriptor) ? descriptor : new ResistanceDescriptor(0, 0);
        }
    }

    public class DefaultCharacterStats : ICharacterStats
    {
        public IStat<int> FlatDamage { get; } = new IntStat(0);
        public IStat<int> Power { get; } = new IntStat(0);
        public IStat<int> CritChance { get; } = new IntStat(0);
        public IStat<int> CritFlatDamage { get; } = new IntStat(0);
        public IStat<float> FinalDamagePercentage { get; } = new FloatStat(0f);
        public IStat<float> AttackSpeed { get; } = new FloatStat(0f);
        public IStat<float> BulletSpeed { get; } = new FloatStat(0f);

        private readonly Dictionary<DamageType, ResistanceDescriptor> _resistances = new()
        {
            { DamageType.Piercing, new ResistanceDescriptor(0, 0) },
            { DamageType.Fire, new ResistanceDescriptor(0, 0) },
            { DamageType.Explosive, new ResistanceDescriptor(0, 0) },
            { DamageType.Poison, new ResistanceDescriptor(0, 0) },
            { DamageType.Slashing, new ResistanceDescriptor(0, 0) }
        };

        public ResistanceDescriptor GetResistances(DamageType damageType)
        {
            return _resistances.TryGetValue(damageType, out var descriptor) ? descriptor : new ResistanceDescriptor(0, 0);
        }
    }

    [Serializable]
    public struct ResistanceDescriptor
    {
        public IStat<int> FlatResistance => flatResistance;
        public IStat<int> PercentageResistance => percentageResistance;
        
        [SerializeField] private IntStat flatResistance;
        [SerializeField] private IntStat percentageResistance;

        public ResistanceDescriptor(int flatRes, int percentRes)
        {
            flatResistance = new IntStat(flatRes);
            percentageResistance = new IntStat(percentRes);
        }
    }
}
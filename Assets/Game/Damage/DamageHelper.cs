using Game.Stats;

namespace Game.Damage
{
    public static class DamageHelper
    {
        private static readonly ICharacterStats DefaultStats = new DefaultCharacterStats();
        
        public static DamageContext ComputeDamageContext(IDamageSource source, int baseDamage, ICharacterStats attackerStats, IDamageTarget target)
        {
            var res = target is IResistanceProvider r ? r.GetResistances(source.GetDamageType()) : DefaultStats.GetResistances(source.GetDamageType());
            
            var computeDamage = Calculator.ComputeDamage(new DamageCalculationContext
            {
                AttackerFinalDamagePercentage = attackerStats.FinalDamagePercentage.Value,
                AttackerFlatDamage = attackerStats.FlatDamage.Value,
                AttackerPower = attackerStats.Power.Value,
                BaseDamage = baseDamage,
                TargetFlatResistance = res.FlatResistance.Value,
                TargetPercentageResistance = res.PercentageResistance.Value,
            });
            
            return new DamageContext(source, computeDamage);
        }
    }
}
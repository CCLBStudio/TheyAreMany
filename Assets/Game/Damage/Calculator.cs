using Game.Stats;
using UnityEngine;

namespace Game.Damage
{
    public static class Calculator
    {
        public static ComputedDamage ComputeDamage(DamageCalculationContext ctx)
        {
            int trueDamage = Mathf.Max(0, Mathf.FloorToInt(ctx.BaseDamage * ComputeBaseDamageMultiplier(ctx.AttackerPower) + ctx.AttackerFlatDamage));
            int reducedDamage = Mathf.FloorToInt(Mathf.Max(0, trueDamage - ctx.TargetFlatResistance) * ComputeResMultiplier(ctx.TargetPercentageResistance));
            int finalDamage = Mathf.Max(1, Mathf.FloorToInt(reducedDamage * ComputeFinalDamageMultiplier(ctx.AttackerFinalDamagePercentage)));
            
            return new ComputedDamage
            {
                TrueDamage = trueDamage,
                ReducedDamage = reducedDamage,
                ReducedAmount = trueDamage - reducedDamage,
                FinalDamage = finalDamage,
                IsCritical = false
            };
        }

        private static float ComputeBaseDamageMultiplier(float power)
        {
            return 1f + power / 100f;
        }
        
        private static float ComputeFinalDamageMultiplier(float finalDamagePercentage)
        {
            return 1f + finalDamagePercentage / 100f;
        }
        
        private static float ComputeResMultiplier(float percentageResistance)
        {
            return 1f - percentageResistance / 100f;
        }
    }
    
    public readonly struct DamageCalculationContext
    {
        public int BaseDamage { get; init; }
        public int AttackerPower { get; init; }
        public int AttackerFlatDamage { get; init; }
        public float AttackerFinalDamagePercentage { get; init; }
        public int TargetFlatResistance { get; init; }
        public float TargetPercentageResistance { get; init; }
    }

    public readonly struct ComputedDamage
    {
        public int TrueDamage { get; init; }
        public int ReducedDamage { get; init; }
        public int ReducedAmount { get; init; }
        public int FinalDamage { get; init; }
        public bool IsCritical { get; init; }
    }
}
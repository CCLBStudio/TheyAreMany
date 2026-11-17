using System;
using UnityEngine;

namespace CCLBStudio.Loot
{
    public static class LootExtensions
    {
        public static WeightedEntry[] FromCurves(ILoot[] loots, AnimationCurve curve)
        {
            if(loots.Length <= 0)
            {
                return Array.Empty<WeightedEntry>();
            }
            
            var entries = new WeightedEntry[loots.Length];
            var last = Mathf.Max(1, loots.Length - 1);
            
            for(int i = 0; i < loots.Length; i++)
            {
                var t = loots.Length == 1 ? 0f : (float)i / last;
                entries[i] = new WeightedEntry {loot = loots[i], weight = curve.Evaluate(t)};
            }
            
            return entries;
        }
    }
}
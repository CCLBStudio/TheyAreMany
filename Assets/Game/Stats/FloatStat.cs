using System;
using UnityEngine;

namespace Game.Stats
{
    [Serializable]
    public class FloatStat : Stat<float>
    {
        public FloatStat() { }
        public FloatStat(float value) : base(value) { }

        protected override float CalculateFinalValue()
        {
            float finalValue = baseValue;

            foreach (var mod in modifiers)
            {
                finalValue += mod.Value;
            }

            return Mathf.Max(0, finalValue);
        }
    }
    
    [Serializable]
    public class IntStat : Stat<int>
    {
        public IntStat() { }
        public IntStat(int value) : base(value) { }

        protected override int CalculateFinalValue()
        {
            int finalValue = baseValue;

            foreach (var mod in modifiers)
            {
                finalValue += mod.Value;
            }

            return Mathf.Max(0, finalValue);
        }
    }
}
using System;

namespace CCLBStudio.Loot
{
    public class PrefixSumPicker : IPicker
    {
        ILoot[] loots;
        float[] cumulative;
        private float total;

        public PickerType Type => PickerType.PrefixSum;

        public void Rebuild(WeightedEntry[] entries)
        {
            loots = new ILoot[entries.Length];
            cumulative = new float[entries.Length];
            total = 0f;
            
            for(int i = 0; i < entries.Length; i++)
            {
                loots[i] = entries[i].loot;
                total += entries[i].weight;
                cumulative[i] = total;
            }
        }

        public ILoot Pick(Random rng)
        {
            if(loots == null || loots.Length == 0 || total <= 0f)
            {
                return null;
            }
            
            float roll = (float)(rng.NextDouble() * total);
            int lo = 0;
            int hi = cumulative.Length - 1;

            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                
                if (cumulative[mid] < roll)
                {
                    lo = mid + 1;
                }
                else
                {
                    hi = mid;
                }
            }
            
            return loots[lo];
        }
    }
}
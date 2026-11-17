using System;

namespace CCLBStudio.Loot
{
    public class AliasSumPicker : IPicker
    {
        public PickerType Type => PickerType.AliasSum;
        
        ILoot[] loots;
        float[] prob;
        private int[] alias;

        public void Rebuild(WeightedEntry[] entries)
        {
            int n = entries.Length;
            loots = new ILoot[n];
            prob = new float[n];
            alias = new int[n];
            float sum = 0f;
            float[] scaled = new float[n];

            for (int i = 0; i < n; i++)
            {
                loots[i] = entries[i].loot;
                scaled[i] = entries[i].weight;
                sum += scaled[i];
            }
            
            if(sum <= 0f || n == 0)
            {
                return;
            }
            
            for(int i = 0; i < n; i++)
            {
                scaled[i] = scaled[i] * n / sum;
            }
            
            int[] small = new int[n];
            int[] large = new int[n];
            int smallCount = 0;
            int largeCount = 0;
            
            for(int i = 0; i < n; i++)
            {
                if(scaled[i] < 1f)
                {
                    small[smallCount++] = i;
                }
                else
                {
                    large[largeCount++] = i;
                }
            }

            while (smallCount > 0 && largeCount > 0)
            {
                int s = small[--smallCount];
                int l = large[--largeCount];
                prob[s] = scaled[s];
                alias[s] = l;
                scaled[l] = (scaled[l] + scaled[s]) - 1f;
                
                if(scaled[l] < 1f)
                {
                    small[smallCount++] = l;
                }
                else
                {
                    large[largeCount++] = l;
                }
            }

            while (largeCount > 0)
            {
                int i = large[--largeCount];
                prob[i] = 1f;
                alias[i] = i;
            }
            
            while( smallCount > 0)
            {
                int i = small[--smallCount];
                prob[i] = 1f;
                alias[i] = i;
            }
        }
        
        public ILoot Pick(Random rng)
        {
            if(loots == null || loots.Length == 0)
            {
                return null;
            }
            
            int i = rng.Next(loots.Length);
            return rng.NextDouble() < prob[i] ? loots[i] : loots[alias[i]];
        }
    }
}
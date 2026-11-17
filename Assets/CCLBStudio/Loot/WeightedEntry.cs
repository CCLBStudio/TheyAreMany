using System;
using UnityEngine;

namespace CCLBStudio.Loot
{
    [Serializable]
    public struct WeightedEntry
    {
        [SerializeReference] public ILoot loot;
        [Min(0f)]
        public float weight;
    }
}
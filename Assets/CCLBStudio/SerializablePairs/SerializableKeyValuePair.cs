using System;
using UnityEngine;

namespace CCLBStudio.SerializablePairs
{
    [Serializable]
    public class SerializableKeyValuePair<TK, TV>
    {
        public TK Key => key;

        public TV Value
        {
            get => value;
            set => this.value = value;
        }

        [SerializeField] private TK key;
        [SerializeField] private TV value;

        public SerializableKeyValuePair(TK key, TV value)
        {
            this.key = key;
            this.value = value;
        }
    }
}

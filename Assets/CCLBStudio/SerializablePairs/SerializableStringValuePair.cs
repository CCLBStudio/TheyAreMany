using System;
using UnityEngine;

namespace CCLBStudio.SerializablePairs
{
    [Serializable]
    public class SerializableStringValuePair<T>
    {
        public string Key
        {
            get => key;
            set => key = value;
        }

        public T Value
        {
            get => value;
            set => this.value = value;
        }

        [SerializeField] private string key;
        [SerializeField] private T value;

        public SerializableStringValuePair(string key, T value)
        {
            this.key = key;
            this.value = value;
        }
    }
}

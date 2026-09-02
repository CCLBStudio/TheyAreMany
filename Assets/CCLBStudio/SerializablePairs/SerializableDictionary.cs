using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CCLBStudio.SerializablePairs
{
    [Serializable]
    public class SerializableDictionary<TK, TV> : IEnumerator, IEnumerable
    {
        #if UNITY_EDITOR

        public static string InitialPairsPropertyName => nameof(initialPairs);
        public static string DebugDisplayPropertyName => nameof(debugDisplay);

        public List<SerializableKeyValuePair<TK, TV>> InitialPairs => initialPairs;
        
        #endif
        
        public int Count => GetCount();
        public Dictionary<TK, TV> Dictionary => GetDictionary();

        [SerializeField] private List<SerializableKeyValuePair<TK, TV>> initialPairs = new();
        [SerializeField] private List<SerializableKeyValuePair<TK, TV>> debugDisplay = new();
        
        private Dictionary<TK, TV> _dictionary;
        [NonSerialized] private bool _init;
        private int _position = -1;

        public SerializableDictionary(SerializableDictionary<TK, TV> copyFrom)
        {
            initialPairs = new List<SerializableKeyValuePair<TK, TV>>(copyFrom.initialPairs);
            debugDisplay = new List<SerializableKeyValuePair<TK, TV>>(copyFrom.debugDisplay);
            _dictionary = new Dictionary<TK, TV>(copyFrom._dictionary);

            _position = -1;
            _init = false;
            CheckInit();
        }

        public SerializableDictionary(Dictionary<TK, TV> copyFrom)
        {
            initialPairs = new List<SerializableKeyValuePair<TK, TV>>();
            
            foreach (var kvp in copyFrom)
            {
                initialPairs.Add(new SerializableKeyValuePair<TK, TV>(kvp.Key, kvp.Value));
            }
            
            _position = -1;
            _init = false;
            CheckInit();
        }

        public SerializableDictionary()
        {
            
        }

        public void ClearNull()
        {
            while (true)
            {
                if (!Application.isPlaying)
                {
                    int index = initialPairs.FindIndex(x => x.Value.Equals(default));
                    if (index < 0)
                    {
                        break;
                    }
                    else
                    {
                        initialPairs.RemoveAt(index);
                    }
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public TV this[TK index]
        {
            get => GetValueAtIndex(index);
            set => SetValueAtIndex(index, value);
        }

        private TV GetValueAtIndex(TK index)
        {
            CheckInit();
            return _dictionary[index];
        }

        private void SetValueAtIndex(TK index, TV value)
        {
            CheckInit();
            _dictionary[index] = value;
            if (Application.isEditor)
            {
                int i = debugDisplay.FindIndex(x => _dictionary.Comparer.Equals(x.Key, index));
                if (i >= 0)
                {
                    debugDisplay[i].Value = value;
                }
            }
        }

        private void CheckInit()
        {
            if (_init)
            {
                return;
            }

            if (Application.isEditor)
            {
                debugDisplay = new List<SerializableKeyValuePair<TK, TV>>(initialPairs);
            }
            
            _dictionary = initialPairs.ToDictionary(x => x.Key, x => x.Value);
            _init = true;
        }

        public bool Add(TK key, TV value)
        {
            if (!Application.isPlaying)
            {
                initialPairs.Add(new SerializableKeyValuePair<TK, TV>(key, value));
                return true;
            }
            
            CheckInit();
            bool success = _dictionary.TryAdd(key, value);

            if (Application.isEditor && success)
            {
                debugDisplay.Add(new SerializableKeyValuePair<TK, TV>(key, value));
            }
            
            return success;
        }

        public bool Remove(TK key)
        {
            if (!Application.isPlaying)
            {
                int index = initialPairs.FindIndex(x => x.Key.Equals(key));
                if (index >= 0)
                {
                    initialPairs.RemoveAt(index);
                }
                
                index = debugDisplay.FindIndex(x => x.Key.Equals(key));
                if (index >= 0)
                {
                    debugDisplay.RemoveAt(index);
                }
                
                return true;
            }
            
            CheckInit();
            bool success = _dictionary.Remove(key);

            if (Application.isEditor && success)
            {
                int index = debugDisplay.FindIndex(x => _dictionary.Comparer.Equals(x.Key, key));
                if (index >= 0)
                {
                    debugDisplay.RemoveAt(index);
                }
            }
            
            return success;
        }

        public bool Remove(TK key, out TV result)
        {
            CheckInit();
            bool success = _dictionary.Remove(key, out TV value);
            
            if (Application.isEditor && success)
            {
                int index = debugDisplay.FindIndex(x => _dictionary.Comparer.Equals(x.Key, key));
                if (index >= 0)
                {
                    debugDisplay.RemoveAt(index);
                }
            }
            
            result = value;
            return success;
        }

        private Dictionary<TK, TV> GetDictionary()
        {
            CheckInit();
            return _dictionary;
        }

        /// <summary>
        /// Will attempt to get the value associated with the specified key. If none exist, will return the default value (null for reference types, default for value types).
        /// </summary>
        /// <param name="key">The dictionary's key to find the value.</param>
        /// <returns>The value associated with the provided key if exists. Default otherwise. </returns>
        public TV Get(TK key)
        {
            TryGet(key, out TV result);
            return result;
        }

        /// <summary>
        /// Will attempt to get the value associated with the specified key. The result will be stored into the out parameter.
        /// </summary>
        /// <param name="key">The dictionary's key to find the object.</param>
        /// <param name="result">The out object in which the result will be stored.</param>
        /// <returns>True if a value is found. Default otherwise (null for reference types, default for value types).</returns>
        public bool TryGet(TK key, out TV result)
        {
            if (!Application.isPlaying)
            {
                int index = initialPairs.FindIndex(x => x.Key.Equals(key));
                result = index >= 0 ? initialPairs[index].Value : default;

                return index >= 0;
            }
            
            CheckInit();
            bool success = _dictionary.TryGetValue(key, out TV value);
            result = value;
            return success;
        }

        /// <summary>
        /// Check if the provided key exists.
        /// </summary>
        /// <param name="key">The key to check</param>
        /// <returns>True if the key exists. False otherwise.</returns>
        public bool ContainsKey(TK key)
        {
            if (!Application.isPlaying)
            {
                return initialPairs.Find(x => x.Key.Equals(key)) != default;
            }
            
            CheckInit();
            return _dictionary.ContainsKey(key);
        }

        /// <summary>
        /// Return all the keys. If executed outside the playmode, return the keys stored into the initialPairs list.
        /// </summary>
        /// <returns>The list of existing keys.</returns>
        public List<TK> GetKeys()
        {
            if (!Application.isPlaying)
            {
                return initialPairs.Select(x => x.Key).ToList();
            }
            
            CheckInit();
            return _dictionary.Keys.ToList();
        }

        /// <summary>
        /// Return all the values. If executed outside the playmode, return the values stored into the initialPairs list.
        /// </summary>
        /// <returns>The list of existing values.</returns>
        public List<TV> GetValuesAsList()
        {
            if (!Application.isPlaying)
            {
                return initialPairs.Select(x => x.Value).ToList();
            }
            
            CheckInit();
            return _dictionary.Values.ToList();
        }

        public IEnumerable<TV> GetValues()
        {
            if (!Application.isPlaying)
            {
                return initialPairs.Select(x => x.Value);
            }
            
            CheckInit();
            return _dictionary.Values;
        }

        public void Clear()
        {
            CheckInit();
            _dictionary.Clear();
            if (Application.isEditor)
            {
                debugDisplay.Clear();
            }
        }

        private int GetCount()
        {
            if (!Application.isPlaying)
            {
                return initialPairs.Count;
            }
            
            CheckInit();
            return _dictionary.Count;
        }

        public bool MoveNext()
        {
            CheckInit();
            _position++;
            return _position < _dictionary.Count;
        }

        public void Reset()
        {
            _position = -1;
        }

        public object Current => GetCurrent();

        public IEnumerator GetEnumerator()
        {
            CheckInit();
            return this;
        }

        private KeyValuePair<TK, TV> GetCurrent()
        {
            CheckInit();
            return _dictionary.ElementAt(_position);
        }
    }
}

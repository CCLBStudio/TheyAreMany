using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CCLBStudio.ScriptableValue
{
    public abstract class ScriptableListValue<T> : ScriptableValue<List<T>>
    {
        public event Action OnCollectionChanged;

        public void Add(T elem)
        {
            value.Add(elem);
            OnCollectionChanged?.Invoke();
        }

        public void AddRange(IEnumerable<T> collection)
        {
            value.AddRange(collection);
            OnCollectionChanged?.Invoke();
        }

        public void Remove(T elem)
        {
            value.Remove(elem);
            OnCollectionChanged?.Invoke();
        }

        public void RemoveAt(int index)
        {
            value.RemoveAt(index);
            OnCollectionChanged?.Invoke();
        }

        public int RemoveAll(Predicate<T> match)
        {
            int result = value.RemoveAll(match);
            if (result > 0)
            {
                OnCollectionChanged?.Invoke();
            }
            
            return result;
        }

        public void RemoveRange(int index, int count)
        {
            value.RemoveRange(index, count);
            OnCollectionChanged?.Invoke();
        }

        public void Clear()
        {
            value.Clear();
            OnCollectionChanged?.Invoke();
        }

        public void Reverse()
        {
            value.Reverse();
            OnCollectionChanged?.Invoke();
        }

        public void Insert(int index, T elem)
        {
            value.Insert(index, elem);
            OnCollectionChanged?.Invoke();
        }

        public void InsertRange(int index, [NotNull] IEnumerable<T> collection)
        {
            value.InsertRange(index, collection);
            OnCollectionChanged?.Invoke();
        }
    }
}
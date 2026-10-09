using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Stats
{
    [Serializable]
    public abstract class Stat<T> : IStat<T>
    {
        [SerializeField] protected T baseValue;

        protected bool isDirty = true;
        protected T lastCalculatedValue;
        protected readonly List<StatModifier<T>> modifiers = new List<StatModifier<T>>();

        static Stat()
        {
            if (typeof(T) != typeof(int) && typeof(T) != typeof(float))
            {
                throw new NotSupportedException(
                    $"Type {typeof(T)} is not authorized for Stat<T>. Only int and float are supported.");
            }
        }

        protected Stat()
        {
            baseValue = default;
        }

        protected Stat(T value)
        {
            baseValue = value;
        }

        public T Value
        {
            get
            {
                if (!isDirty)
                {
                    return lastCalculatedValue;
                }

                lastCalculatedValue = CalculateFinalValue();
                isDirty = false;
                return lastCalculatedValue;
            }
        }

        public void AddModifier(StatModifier<T> modifier)
        {
            modifiers.Add(modifier);
            isDirty = true;
        }

        public bool RemoveModifier(StatModifier<T> modifier)
        {
            if (modifiers.Remove(modifier))
            {
                isDirty = true;
                return true;
            }

            return false;
        }

        public bool RemoveAllModifiersFromSource(object source)
        {
            int removed = modifiers.RemoveAll(mod => mod.Source == source);
            if (removed > 0)
            {
                isDirty = true;
                return true;
            }

            return false;
        }

        protected abstract T CalculateFinalValue();
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CCLBStudio.GlobalUpdater
{
    internal abstract class Updater<T>
    {
        protected static HashSet<T> updatables = new();
        protected static T[] buffer = Array.Empty<T>();
        protected static bool requireUpdatableFlush;

        internal static void RegisterUpdatable(T updatable)
        {
            if (updatable == null)
            {
                return;
            }
            
            updatables.Add(updatable);
        }

        internal static void UnregisterUpdatable(T updatable)
        {
            if (updatable == null)
            {
                return;
            }
            
            updatables.Remove(updatable);
        }

        internal static void Clear()
        {
            updatables.Clear();
            buffer = Array.Empty<T>();
        }

        protected static int PrepareBuffer()
        {
            int count = updatables.Count;

            if (buffer.Length < count)
            {
                buffer = new T[count];
            }

            updatables.CopyTo(buffer);
            return count;
        }

        protected static bool IsNull(T obj)
        {
            if (obj is UnityEngine.Object unityObj)
            {
                return !unityObj;
            }
            
            return obj == null;
        }

        protected static void FlushUpdatables()
        {
            Debug.Log("Flushing updatables");
            var newSet = new HashSet<T>(updatables.Count);
            foreach (var u in updatables)
            {
                if (!IsNull(u))
                {
                    newSet.Add(u);
                }
            }

            updatables = newSet;
            requireUpdatableFlush = false;

            if (buffer.Length < updatables.Count)
            {
                buffer = new T[updatables.Count];
            }
            
            Debug.Log($"Updatables has now {updatables.Count} elements");
        }
    }
}

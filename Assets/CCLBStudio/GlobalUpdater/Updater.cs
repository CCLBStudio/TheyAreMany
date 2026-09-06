using System;
using System.Collections.Generic;
using UnityEngine;

namespace CCLBStudio.GlobalUpdater
{
    internal abstract class Updater<T>
    {
        protected static HashSet<T> updates = new();
        protected static T[] buffer = Array.Empty<T>();
        protected static bool requireUpdateFlush;

        internal static void RegisterUpdate(T u)
        {
            if (u == null)
            {
                return;
            }
            
            updates.Add(u);
        }

        internal static void UnregisterUpdate(T u)
        {
            if (u == null)
            {
                return;
            }
            
            updates.Remove(u);
        }

        internal static void Clear()
        {
            updates.Clear();
            buffer = Array.Empty<T>();
        }

        protected static int PrepareBuffer()
        {
            int count = updates.Count;

            if (buffer.Length < count)
            {
                buffer = new T[count];
            }

            updates.CopyTo(buffer);
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

        protected static void FlushUpdates()
        {
            Debug.Log("Flushing Updates");
            var newSet = new HashSet<T>(updates.Count);
            foreach (var u in updates)
            {
                if (!IsNull(u))
                {
                    newSet.Add(u);
                }
            }

            updates = newSet;
            requireUpdateFlush = false;

            if (buffer.Length < updates.Count)
            {
                buffer = new T[updates.Count];
            }
            
            Debug.Log($"Updates has now {updates.Count} elements");
        }
    }
}

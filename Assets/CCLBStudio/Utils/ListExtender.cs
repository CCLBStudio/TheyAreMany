using System;
using System.Collections.Generic;
using System.Linq;
using Object = UnityEngine.Object;
using Random = System.Random;

namespace CCLBStudio.Utils
{
    public static class ListExtender
    {
        public static void ClearNull<T>(this List<T> e)
        {
            if (typeof(T).IsAssignableFrom(typeof(Object)))
            {
                e.RemoveAll(x => !(x as Object));
                return;
            }

            e.RemoveAll(x => x == null);
        }
        
        public static IEnumerable<T> WithoutNull<T>(this IEnumerable<T> source)
        {
            return typeof(T).IsAssignableFrom(typeof(Object)) ? source.Where(x => x as Object) : source.Where(x => x != null);
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            Random rng = new Random();

            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);  
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        public static bool TryPeek<T>(this List<T> list, out T result)
        {
            if (list == null)
            {
                throw new NullReferenceException("List is null");
            }

            result = default;
            if (list.Count <= 0)
            {
                return false;
            }

            result = list[^1];
            return true;
        }
        
        public static bool TryPop<T>(this List<T> list, out T result)
        {
            bool success = list.TryPeek(out T r);
            result = r;
            
            if (success)
            {
                list.RemoveAt(list.Count - 1);
            }

            return success;
        }
    }
}

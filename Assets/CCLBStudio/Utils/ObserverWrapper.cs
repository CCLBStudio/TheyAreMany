using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Utils
{
    public class ObserverWrapper<T> where T : class
    {
        public int ListenerCount => _listeners.Count;
        
        [NonSerialized] private readonly List<T> _listeners = new();
        [NonSerialized] private readonly Dictionary<string, MethodInfo> _knownMethods = new();
        [NonSerialized] private int _notifyCount = 0;
        private event Action PostNotification;

        #region Observer Methods

        /// <summary>
        /// Add the specified listener to the list, if it is not already present.
        /// </summary>
        /// <param name="l">The listener to add.</param>
        public void AddListener(T l)
        {
            if (_notifyCount > 0)
            {
                PostNotification += () => AddListener(l);
                return;
            }
        
            if (_listeners.Contains(l))
            {
                return;
            }

            _listeners.Add(l);
        }

        /// <summary>
        /// Remove the specified listener from the list, if it exists.
        /// </summary>
        /// <param name="l">The listener to remove.</param>
        public void RemoveListener(T l)
        {
            if (_notifyCount > 0)
            {
                PostNotification += () => RemoveListener(l);
                return;
            }
        
            int index = _listeners.FindIndex(x => x.Equals(l));
            if (index < 0)
            {
                return;
            }

            _listeners.RemoveAt(index);
        }

        /// <summary>
        /// Fully clear the listeners, removing them all from the list.
        /// </summary>
        public void ClearListeners()
        {
            _listeners.Clear();
        }

        public void NotifyListeners(string methodName, params object[] parameters)
        {
            MethodInfo methodInfo = GetMethod(methodName);
            if (methodInfo == null)
            {
                Debug.LogError($"No suitable method named {methodName} found.");
                return;
            }

            ClearNullListeners();
            _notifyCount++;

            try
            {
                foreach (var l in _listeners)
                {
                    //App.Instance.Log.Verbose($"--- OBSERVER --- Invoking method {methodName} on object {l.GetType().Name}. Parameter(s) : {(parameters == null ? "none" : string.Join(',', parameters))}.");
                    methodInfo.Invoke(l, parameters);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occured during observer event firing : {e}");
            }

            _notifyCount--;
            CheckForPostNotification();
        }

        #endregion

        #region Private Methods

        private void CheckForPostNotification()
        {
            if (_notifyCount > 0)
            {
                return;
            }
            
            PostNotification?.Invoke();
            PostNotification = null;
        }

        private MethodInfo GetMethod(string methodName)
        {
            MethodInfo methodInfo;
            if (_knownMethods.TryGetValue(methodName, out var cachedMethod))
            {
                methodInfo = cachedMethod;
            }
            else
            {
                methodInfo = typeof(T).GetMethod(methodName);
                if (methodInfo == null)
                {
                    return null;
                }

                _knownMethods[methodName] = methodInfo;
            }
            
            return methodInfo;
        }

        private void ClearNullListeners()
        {
            if (_listeners.Any(x => x == null))
            {
                _listeners.RemoveAll(x => x == null);
            }
        }

        #endregion
    }
}

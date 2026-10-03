using System;
using System.Collections.Generic;
using UnityEngine;

namespace CCLBStudio.EventBus
{
    /// <summary>
    /// Keeps track of every generic bus that has been used so they can all be reset when entering play mode
    /// (required when domain reload is disabled).
    /// </summary>
    internal static class EventBusRegistry
    {
        private static readonly List<Action> ClearActions = new();

        internal static void Register(Action clearAction)
        {
            ClearActions.Add(clearAction);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void ClearAll()
        {
            for (int i = 0; i < ClearActions.Count; i++)
            {
                ClearActions[i].Invoke();
            }
        }
    }
}

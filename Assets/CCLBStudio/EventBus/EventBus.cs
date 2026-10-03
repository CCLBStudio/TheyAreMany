using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CCLBStudio.EventBus
{
    /// <summary>
    /// Static, per-event-type bus. Each event type gets its own storage through the generic static class,
    /// so there is no dictionary lookup, no boxing and no allocation when raising an event.
    /// Main thread only.
    /// </summary>
    /// <example>
    /// <code>
    /// public struct EnemyKilled : IEvent { public int EnemyId; public Vector3 Position; }
    ///
    /// public class Score : MonoBehaviour, IEventListener&lt;EnemyKilled&gt;
    /// {
    ///     private void OnEnable() => EventBus&lt;EnemyKilled&gt;.Subscribe(this);
    ///     private void OnDisable() => EventBus&lt;EnemyKilled&gt;.Unsubscribe(this);
    ///     public void OnEvent(in EnemyKilled evt) { ... }
    /// }
    ///
    /// EventBus&lt;EnemyKilled&gt;.Raise(new EnemyKilled { EnemyId = 3, Position = pos });
    /// </code>
    /// </example>
    public static class EventBus<T> where T : struct, IEvent
    {
        private static readonly ListenerCollection<IEventListener<T>> Listeners = new();
        private static readonly ListenerCollection<EventCallback<T>> Callbacks = new();

        static EventBus()
        {
            EventBusRegistry.Register(Clear);
        }

        public static int ListenerCount => Listeners.Count + Callbacks.Count;

        public static EventSubscription<T> Subscribe(IEventListener<T> listener)
        {
            Listeners.Add(listener);
            return new EventSubscription<T>(listener);
        }

        public static EventSubscription<T> Subscribe(EventCallback<T> callback)
        {
            Callbacks.Add(callback);
            return new EventSubscription<T>(callback);
        }

        public static bool Unsubscribe(IEventListener<T> listener)
        {
            return Listeners.Remove(listener);
        }

        public static bool Unsubscribe(EventCallback<T> callback)
        {
            return Callbacks.Remove(callback);
        }

        public static bool IsSubscribed(IEventListener<T> listener)
        {
            return Listeners.Contains(listener);
        }

        public static bool IsSubscribed(EventCallback<T> callback)
        {
            return Callbacks.Contains(callback);
        }

        public static void Raise(T evt)
        {
            Raise(in evt);
        }

        /// <summary>
        /// Dispatches the event to every listener, then to every callback, in subscription order.
        /// Listeners subscribed during the dispatch will only receive the next events.
        /// Destroyed Unity objects are automatically unsubscribed.
        /// </summary>
        public static void Raise(in T evt)
        {
            RaiseListeners(in evt);
            RaiseCallbacks(in evt);
        }

        public static void Clear()
        {
            Listeners.Clear();
            Callbacks.Clear();
        }

        private static void RaiseListeners(in T evt)
        {
            int count = Listeners.BeginIteration();
            try
            {
                for (int i = 0; i < count; i++)
                {
                    IEventListener<T> listener = Listeners.Items[i];
                    if (listener == null)
                    {
                        continue;
                    }

                    if (listener is Object unityObject && !unityObject)
                    {
                        Listeners.RemoveAt(i);
                        continue;
                    }

                    try
                    {
                        listener.OnEvent(in evt);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e, listener as Object);
                    }
                }
            }
            finally
            {
                Listeners.EndIteration();
            }
        }

        private static void RaiseCallbacks(in T evt)
        {
            int count = Callbacks.BeginIteration();
            try
            {
                for (int i = 0; i < count; i++)
                {
                    EventCallback<T> callback = Callbacks.Items[i];
                    if (callback == null)
                    {
                        continue;
                    }

                    if (callback.Target is Object unityObject && !unityObject)
                    {
                        Callbacks.RemoveAt(i);
                        continue;
                    }

                    try
                    {
                        callback.Invoke(in evt);
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e, callback.Target as Object);
                    }
                }
            }
            finally
            {
                Callbacks.EndIteration();
            }
        }
    }

    /// <summary>
    /// Non-generic facade allowing type inference: <c>Events.Raise(new EnemyKilled { ... });</c>
    /// Not named EventBus to avoid clashing with the CCLBStudio.EventBus namespace.
    /// </summary>
    public static class Events
    {
        public static void Raise<T>(in T evt) where T : struct, IEvent => EventBus<T>.Raise(in evt);
        public static void Raise<T>(T evt) where T : struct, IEvent => EventBus<T>.Raise(in evt);

        public static EventSubscription<T> Subscribe<T>(IEventListener<T> listener) where T : struct, IEvent => EventBus<T>.Subscribe(listener);
        public static EventSubscription<T> Subscribe<T>(EventCallback<T> callback) where T : struct, IEvent => EventBus<T>.Subscribe(callback);

        public static bool Unsubscribe<T>(IEventListener<T> listener) where T : struct, IEvent => EventBus<T>.Unsubscribe(listener);
        public static bool Unsubscribe<T>(EventCallback<T> callback) where T : struct, IEvent => EventBus<T>.Unsubscribe(callback);

        public static void ClearAll() => EventBusRegistry.ClearAll();
    }
}

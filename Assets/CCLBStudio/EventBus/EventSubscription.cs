using System;

namespace CCLBStudio.EventBus
{
    /// <summary>
    /// Allocation-free subscription handle. Dispose it to unsubscribe.
    /// </summary>
    public readonly struct EventSubscription<T> : IDisposable where T : struct, IEvent
    {
        private readonly IEventListener<T> _listener;
        private readonly EventCallback<T> _callback;

        internal EventSubscription(IEventListener<T> listener)
        {
            _listener = listener;
            _callback = null;
        }

        internal EventSubscription(EventCallback<T> callback)
        {
            _listener = null;
            _callback = callback;
        }

        public bool IsValid => _listener != null || _callback != null;

        public void Dispose()
        {
            if (_listener != null)
            {
                EventBus<T>.Unsubscribe(_listener);
            }
            else if (_callback != null)
            {
                EventBus<T>.Unsubscribe(_callback);
            }
        }
    }
}

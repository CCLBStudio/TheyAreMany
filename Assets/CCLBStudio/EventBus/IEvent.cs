namespace CCLBStudio.EventBus
{
    /// <summary>
    /// Marker interface for events. Events must be structs to avoid heap allocations when raised.
    /// </summary>
    public interface IEvent
    {
    }
}

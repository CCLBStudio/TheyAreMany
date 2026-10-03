namespace CCLBStudio.EventBus
{
    /// <summary>
    /// Callback receiving the event by readonly reference, so large event structs are never copied.
    /// </summary>
    public delegate void EventCallback<T>(in T evt) where T : struct, IEvent;
}

namespace CCLBStudio.EventBus
{
    public interface IEventListener<T> where T : struct, IEvent
    {
        void OnEvent(in T evt);
    }
}

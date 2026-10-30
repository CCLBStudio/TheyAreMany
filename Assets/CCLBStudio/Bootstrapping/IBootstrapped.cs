namespace CCLBStudio.Bootstrapping
{
    public interface IBootstrapped
    {
        public int ExecutionOrder => 0;
        public void Initialize();
    }
}
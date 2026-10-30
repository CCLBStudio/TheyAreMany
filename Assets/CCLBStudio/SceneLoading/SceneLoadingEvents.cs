using CCLBStudio.EventBus;

namespace CCLBStudio.SceneLoading
{
    public struct WillUnloadScene : IEvent
    {
        public string SceneName { get; }

        public WillUnloadScene(string sceneName)
        {
            SceneName = sceneName;
        }
    }
    
    public struct SceneUnloaded : IEvent
    {
        public string SceneName { get; }

        public SceneUnloaded(string sceneName)
        {
            SceneName = sceneName;
        }
    }
    
    public struct WillLoadScene : IEvent
    {
        public string SceneName { get; }

        public WillLoadScene(string sceneName)
        {
            SceneName = sceneName;
        }
    }
    
    public struct SceneLoaded : IEvent
    {
        public string SceneName { get; }

        public SceneLoaded(string sceneName)
        {
            SceneName = sceneName;
        }
    }
}
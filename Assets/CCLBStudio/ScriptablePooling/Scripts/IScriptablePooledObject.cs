namespace CCLBStudio.ScriptablePooling
{
    public interface IScriptablePooledObject
    {
        public ScriptablePool Pool { get; set; }
        public void OnObjectCreated();
        public void OnObjectRequested();
        public void OnObjectReleased();
    }
}

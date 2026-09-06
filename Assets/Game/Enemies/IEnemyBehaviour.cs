public interface IEnemyBehaviour
{
    public EnemyFacade Facade { get; set; }
    public bool AutoRegisterToGlobalUpdater() => true;
    public void OnEnemyCreated();
    public void OnEnemyRequested();
    public void OnEnemyReleased();
}

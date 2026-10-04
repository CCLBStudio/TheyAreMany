namespace Game.Player
{
    public interface IPlayerBehaviour
    {
        public PlayerFacade Facade { get; set; }
        public void Initialize();
    }
}

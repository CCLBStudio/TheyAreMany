using CCLBStudio.EventBus;

namespace Game.Player.Health
{
    public struct PlayerKilled : IEvent
    {
        public PlayerKilled(PlayerFacade player, IDamageSource damageSource)
        {
            Player = player;
            DamageSource = damageSource;
        }
        
        public PlayerFacade Player { get; }
        public IDamageSource DamageSource { get; }
    }
    
    public struct PlayerDamaged : IEvent
    {
        public PlayerDamaged(PlayerHealth health, IDamageSource damageSource)
        {
            Health = health;
            DamageSource = damageSource;
        }
        
        public PlayerHealth Health { get; }
        public IDamageSource DamageSource { get; }
    }
}
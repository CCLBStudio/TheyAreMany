using Game.Stats;

namespace Game.ModularWeapon
{
    public interface IRuntimeBulletContext
    {
        public int BaseDamage { get; }
        public float Speed { get; }
        public float LifeTime { get; }
        public ICharacterStats OwnerStats { get; }
    }
    
    public struct RuntimeBulletContext : IRuntimeBulletContext
    {
        public int BaseDamage { get; }
        public float Speed { get; }
        public float LifeTime { get; }
        public ICharacterStats OwnerStats { get; }

        public RuntimeBulletContext(int baseDamage, float speed, float lifeTime, ICharacterStats ownerStats)
        {
            BaseDamage = baseDamage;
            Speed = speed;
            LifeTime = lifeTime;
            OwnerStats = ownerStats;
        }
    }
}
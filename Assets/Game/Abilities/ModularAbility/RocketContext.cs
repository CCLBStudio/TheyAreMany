using CCLBStudio.ScriptablePooling;
using Game.Stats;
using PrimeTween;

namespace Game.ModularAbility
{
    public struct RocketContext
    {
        public int BaseDamage { get; set; }
        public float TravelSpeed { get; set; }
        public float AccelerationTime { get; set; }
        public Ease AccelerationEase { get; set; }
        public float Lifetime { get; set; }
        public ScriptablePool ExplosionPool { get; set; }
        public float ExplosionRange { get; set; }
        public ICharacterStats OwnerStats { get; set; }
    }
}

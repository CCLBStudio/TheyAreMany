using CCLBStudio.EventBus;
using Game.Damage;

namespace Game.Objective
{
    public struct ObjectiveDamaged : IEvent
    {
        public IDamageContext DamageContext { get; }
        public ObjectiveDamaged(IDamageContext damageContext)
        {
            DamageContext = damageContext;
        }
    }
    public struct ObjectiveDestroyed : IEvent
    {
        public IDamageContext DamageContext { get; }
        public ObjectiveDestroyed(IDamageContext damageContext)
        {
            DamageContext = damageContext;
        }
    }
}
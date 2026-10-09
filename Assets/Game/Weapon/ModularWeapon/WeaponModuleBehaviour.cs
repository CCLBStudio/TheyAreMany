using Game.Stats;

namespace Game.ModularWeapon
{
    public abstract class WeaponModuleBehaviour
    {
        protected RuntimeWeapon weapon;

        public virtual void Initialize(RuntimeWeapon runtimeWeapon)
        {
            weapon = runtimeWeapon;
        }
        
        public virtual void OnStartShooting() { }
        public virtual void OnStopShooting() { }
        public virtual void Tick() { }
        public virtual void OnUnequipped(){}
    }

    public abstract class WeaponEffectBehaviour
    {
        protected RuntimeWeapon weapon;

        public virtual void Initialize(RuntimeWeapon runtimeWeapon)
        {
            weapon = runtimeWeapon;
        }

        public abstract void Trigger();
    }
}
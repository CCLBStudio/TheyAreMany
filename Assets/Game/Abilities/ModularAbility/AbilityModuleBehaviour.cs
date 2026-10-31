namespace Game.ModularAbility
{
    public abstract class AbilityModuleBehaviour
    {
        protected RuntimeAbility ability;

        public virtual void Initialize(RuntimeAbility runtimeAbility)
        {
            ability = runtimeAbility;
        }

        public virtual void OnInputPressed() { }
        public virtual void OnInputReleased() { }
        public virtual void Tick() { }
        public virtual void OnUnequipped() { }
    }

    public abstract class AbilityEffectBehaviour
    {
        protected RuntimeAbility ability;

        public virtual void Initialize(RuntimeAbility runtimeAbility)
        {
            ability = runtimeAbility;
        }

        public abstract void Trigger();
    }
}

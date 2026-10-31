namespace Game.ModularAbility
{
    public interface IAbilityModule
    {
        public AbilityModuleBehaviour CreateModuleBehaviour();
    }

    public interface IAbilityEffect
    {
        public AbilityEffectBehaviour CreateEffectBehaviour();
    }
}

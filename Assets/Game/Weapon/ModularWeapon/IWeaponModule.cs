namespace Game.ModularWeapon
{
    public interface IWeaponModule
    {
        public WeaponModuleBehaviour CreateModuleBehaviour();
    }

    public interface IWeaponEffect
    {
        public WeaponEffectBehaviour CreateEffectBehaviour();
    }
}
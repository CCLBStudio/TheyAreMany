using Game.Stats;

namespace Game.Damage
{
    public interface IResistanceProvider
    {
        public ResistanceDescriptor GetResistances(DamageType damageType);
    }
}
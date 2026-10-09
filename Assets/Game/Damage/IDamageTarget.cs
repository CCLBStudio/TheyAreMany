using Game.Stats;
using UnityEngine;

namespace Game.Damage
{
    public interface IDamageTarget
    {
        public ResistanceDescriptor GetResistances(DamageType damageType);
        public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint = null);
    }

    public interface IHealth : IDamageTarget
    {
        public void Heal(float amount);
    }
}
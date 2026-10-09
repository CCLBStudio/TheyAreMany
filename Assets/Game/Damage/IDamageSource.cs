using UnityEngine;

namespace Game.Damage
{
    public interface IDamageSource
    {
        public Vector3 GetPosition();
        public DamageType GetDamageType();
    }

    public interface IDamageContext
    {
        public IDamageSource Source();
        public int Amount();
    }

    public readonly struct DamageContext : IDamageContext
    {
        private readonly IDamageSource _damageSource;
        private readonly int _amount;

        public DamageContext(IDamageSource damageSource, ComputedDamage damageAmount)
        {
            _damageSource = damageSource;
            _amount = damageAmount.FinalDamage;
        }

        public IDamageSource Source()
        {
            return _damageSource;
        }

        public int Amount()
        {
            return _amount;
        }
    }

    public class DebugDamageSource : IDamageSource
    {
        private readonly Vector3 _position;
        private readonly DamageType _damageType;

        public DebugDamageSource(Vector3 position, DamageType damageType)
        {
            _position = position;
            _damageType = damageType;
        }

        public Vector3 GetPosition()
        {
            return _position;
        }

        public DamageType GetDamageType()
        {
            return _damageType;
        }
    }
}
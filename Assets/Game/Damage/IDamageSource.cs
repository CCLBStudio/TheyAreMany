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
        public IDamageSource Source { get; init; }
        public ComputedDamage Damage { get; init; }
    }

    public readonly struct DamageContext : IDamageContext
    {
        public IDamageSource Source { get; init; }
        public ComputedDamage Damage { get; init; }

        public DamageContext(IDamageSource source, ComputedDamage damageAmount)
        {
            Source = source;
            Damage = damageAmount;
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
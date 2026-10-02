using System;
using CCLBStudio.ScriptableValue;
using CCLBStudio.Utils;
using UnityEngine;

namespace Game.ModularWeapon
{
    [Serializable]
    public class KnockbackWeaponEffect : IWeaponEffect
    {
        [SerializeField] private FloatValue force;
        [SerializeField] private Vector3Value inAirMultiplier;
        
        public WeaponEffectBehaviour CreateEffectBehaviour()
        {
            return new KnockbackWeaponEffectBehaviour(new KnockbackContext
            {
                Force = force,
                InAirMultiplier = inAirMultiplier
            });
        }
        
    }
    
    internal struct KnockbackContext
    {
        public FloatValue Force { get; set; }
        public Vector3Value InAirMultiplier { get; set; }
    }
    
    public class KnockbackWeaponEffectBehaviour : WeaponEffectBehaviour
    {
        private readonly FloatValue _force;
        private readonly Vector3Value _inAirMultiplier;
        private IKnockbackTarget _target;
        
        internal KnockbackWeaponEffectBehaviour(KnockbackContext context)
        {
            _force = context.Force;
            _inAirMultiplier = context.InAirMultiplier;
        }

        public override void Initialize(RuntimeWeapon runtimeWeapon)
        {
            base.Initialize(runtimeWeapon);
            if (weapon.Owner is IKnockbackTarget t)
            {
                _target = t;
            }
            else
            {
                Debug.LogError($"Weapon owner {weapon.Owner} does not implement IKnockbackTarget. Knockback effect will not work.");
            }
        }

        public override void Trigger()
        {
            if (_target == null)
            {
                return;
            }
            
            Vector2 dir = weapon.ShootingDirection.ApplyRotation(180f).normalized;
            _target.ApplyKnockback(dir * _force.Value, _inAirMultiplier.Value);
        }
    }
}
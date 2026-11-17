using System;
using UnityEngine;

namespace Game.Damage
{
    public interface IDamageTarget
    {
        public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint = null);
    }

    public interface IHealth : IDamageTarget
    {
        public event Action<IDamageContext> OnDeath;
        public bool IsMaxHealth { get; }
        public float CurrentHealth { get; }

        public void Heal(float amount);
    }
}
using Game.Stats;
using UnityEngine;

namespace Game.Weapon
{
    public interface IWeaponOwner
    {
        public Transform WeaponContainer { get; }
        public ICharacterStats Stats { get; }
    }
}

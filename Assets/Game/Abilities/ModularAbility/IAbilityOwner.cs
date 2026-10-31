using Game.Stats;
using UnityEngine;

namespace Game.ModularAbility
{
    public interface IAbilityOwner
    {
        public Transform AbilityContainer { get; }
        public ICharacterStats Stats { get; }
    }
}

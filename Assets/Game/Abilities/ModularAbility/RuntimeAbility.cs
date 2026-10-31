using System.Collections.Generic;
using CCLBStudio.GlobalUpdater;
using UnityEngine;

namespace Game.ModularAbility
{
    public class RuntimeAbility : MonoBehaviour, IFixedUpdate
    {
        public Vector2 AimDirection { get; private set; } = Vector2.down;
        public IAbilityOwner Owner { get; private set; }
        public Transform AbilityOrigin => abilityOrigin ? abilityOrigin : transform;

        [SerializeField] private Transform abilityOrigin;

        private readonly List<AbilityModuleBehaviour> _abilityBehaviours = new();
        private bool _equipped;

        private void Start()
        {
            GlobalUpdater.RegisterFixedUpdate(this);
        }

        public void Initialize(IAbilityModule[] abilityModules, IAbilityOwner owner)
        {
            Owner = owner;

            foreach (var module in abilityModules)
            {
                var behaviour = module.CreateModuleBehaviour();
                behaviour.Initialize(this);
                _abilityBehaviours.Add(behaviour);
            }

            _equipped = true;
        }

        public void FixedTick()
        {
            foreach (var b in _abilityBehaviours)
            {
                b.Tick();
            }
        }

        public void Aim(Vector2 direction)
        {
            AimDirection = direction == Vector2.zero ? Vector2.down : direction.normalized;
        }

        public void PressInput()
        {
            foreach (var b in _abilityBehaviours)
            {
                b.OnInputPressed();
            }
        }

        public void ReleaseInput()
        {
            foreach (var b in _abilityBehaviours)
            {
                b.OnInputReleased();
            }
        }

        public void Unequip()
        {
            if (!_equipped)
            {
                return;
            }

            foreach (var b in _abilityBehaviours)
            {
                b.OnUnequipped();
            }

            _equipped = false;
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            GlobalUpdater.UnregisterFixedUpdate(this);
            Unequip();
        }
    }
}
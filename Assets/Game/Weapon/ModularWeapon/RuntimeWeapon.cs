using System.Collections.Generic;
using CCLBStudio.GlobalUpdater;
using Game.Weapon;
using UnityEngine;

namespace Game.ModularWeapon
{
    public class RuntimeWeapon : MonoBehaviour, IFixedUpdate
    {
        public Vector2 ShootingDirection { get; set; }
        public IWeaponOwner Owner { get; private set; }
        public Transform BulletOrigin => bulletOrigin;
        public Transform MuzzleOrigin => muzzleOrigin;
        public Transform CasingOrigin => casingOrigin;
        public Animator Animator => animator;

        [SerializeField] private Transform bulletOrigin;
        [SerializeField] private Transform muzzleOrigin;
        [SerializeField] private Transform casingOrigin;
        [SerializeField] private Animator animator;
        
        private readonly List<WeaponModuleBehaviour> _weaponBehaviours = new();
        private bool _equipped;

        private void Start()
        {
            GlobalUpdater.RegisterFixedUpdate(this);
        }

        public void Initialize(IWeaponModule[] weaponModules, IWeaponOwner owner)
        {
            Owner = owner;
            
            foreach (var module in weaponModules)
            {
                var behaviour = module.CreateModuleBehaviour();
                behaviour.Initialize(this);
                _weaponBehaviours.Add(behaviour);
            }

            _equipped = true;
        }

        public void FixedTick()
        {
            foreach (var b in _weaponBehaviours)
            {
                b.Tick();
            }
        }
        
        public void StartShooting()
        {
            foreach (var b in _weaponBehaviours)
            {
                b.OnStartShooting();
            }
        }
        
        public void StopShooting()
        {
            foreach (var b in _weaponBehaviours)
            {
                b.OnStopShooting();
            }
        }
        
        public void Unequip()
        {
            if(!_equipped)
            {
                return;
            }
            
            foreach (var b in _weaponBehaviours)
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
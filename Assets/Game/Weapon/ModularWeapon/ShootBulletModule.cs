using System;
using System.Collections.Generic;
using System.Linq;
using CCLBStudio.ScriptablePooling;
using CCLBStudio.ScriptableValue;
using CCLBStudio.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.ModularWeapon
{
    [Serializable]
    public class ShootBulletModule : IWeaponModule
    {
        [SerializeField] private ScriptablePool bulletPool;
        [SerializeField] private RangeInt baseDamage = new(4, 6);
        [SerializeField] private FloatValue baseFireRate;
        [SerializeField] private FloatValue baseSpread;
        [SerializeField] private FloatValue baseBulletSpeed;
        [SerializeField] private FloatValue baseLifetime;
        [SerializeReference] private IWeaponEffect[] onShootEffects = Array.Empty<IWeaponEffect>();
        
        public WeaponModuleBehaviour CreateModuleBehaviour()
        {
            var ctx = new ShootBulletContext
            {
                BulletPool = bulletPool,
                Damage = baseDamage,
                FireRate = baseFireRate,
                Spread = baseSpread,
                BulletSpeed = baseBulletSpeed,
                OnShootEffects = onShootEffects,
                Lifetime = baseLifetime,
            };
            
            var behaviour = new ShootBulletModuleBehaviour(ctx);
            return behaviour;
        }
    }

    public struct ShootBulletContext
    {
        public ScriptablePool BulletPool { get; set; }
        public FloatValue FireRate { get; set; }
        public FloatValue Spread { get; set; }
        public RangeInt Damage { get; set; }
        public FloatValue BulletSpeed { get; set; }
        public IWeaponEffect[] OnShootEffects { get; set; }
        public FloatValue Lifetime { get; set; }
    }

    public class ShootBulletModuleBehaviour : WeaponModuleBehaviour, IRuntimeBulletStat
    {
        private readonly ScriptablePool _bulletPool;
        private readonly FloatValue _baseFireRate;
        private readonly FloatValue _baseSpread;
        private readonly FloatValue _baseBulletSpeed;
        private readonly RangeInt _baseDamage;
        private readonly FloatValue _baseLifetime;
        private readonly WeaponEffectBehaviour[] _onShootBehaviours;
        
        private bool _isShooting;
        private float _timer;
        
        public ShootBulletModuleBehaviour(ShootBulletContext context)
        {
            _bulletPool = context.BulletPool;
            _baseFireRate = context.FireRate;
            _baseSpread = context.Spread;
            _baseBulletSpeed = context.BulletSpeed;
            _baseDamage = context.Damage;
            _onShootBehaviours = context.OnShootEffects.Select(x => x.CreateEffectBehaviour()).ToArray();
            _baseLifetime = context.Lifetime;
        }

        public override void Initialize(RuntimeWeapon runtimeWeapon)
        {
            base.Initialize(runtimeWeapon);
            foreach (var e in _onShootBehaviours)
            {
                e.Initialize(runtimeWeapon);
            }
        }

        private Quaternion ComputeSpread(float dispersion, Vector2 shootingDirection)
        {
            var orientation = Quaternion.FromToRotation(Vector3.right, shootingDirection) * Quaternion.AngleAxis(dispersion, Vector3.forward);
            return orientation;
        }
        
        private void SpawnBullet(Vector2 shootingDirection)
        {
            var bullet = _bulletPool.RequestObjectAs<RuntimeBullet>();
            float dispersion = Random.Range(-_baseSpread.Value, _baseSpread.Value);
            bullet.Direction = shootingDirection.ApplyRotation(dispersion);
            bullet.transform.SetPositionAndRotation(weapon.BulletOrigin.position, ComputeSpread(dispersion, shootingDirection));
            
            bullet.Initialize(this);
        }
        
        public override void OnStartShooting()
        {
            _isShooting = true;
        }

        public override void OnStopShooting()
        {
            _isShooting = false;
        }

        public override void Tick()
        {
            _timer += Time.deltaTime;

            if (!_isShooting || _timer < 1f / _baseFireRate.Value)
            {
                return;
            }
            
            _timer = 0f;
            
            SpawnBullet(weapon.ShootingDirection);
            
            foreach (var behaviour in _onShootBehaviours)
            {
                behaviour.Trigger();
            }
        }

        public int GetDamage()
        {
            return _baseDamage.GetRandom();
        }

        public float GetSpeed()
        {
            return _baseBulletSpeed.Value;
        }

        public float GetLifetime()
        {
            return _baseLifetime.Value;
        }
    }
}
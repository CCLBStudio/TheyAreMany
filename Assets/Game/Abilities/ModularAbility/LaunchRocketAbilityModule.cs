using System;
using System.Linq;
using CCLBStudio.ScriptablePooling;
using PrimeTween;
using UnityEngine;

namespace Game.ModularAbility
{
    [Serializable]
    public class LaunchRocketAbilityModule : IAbilityModule
    {
        [SerializeField] private ScriptablePool rocketPool;
        [SerializeField] private ScriptablePool explosionPool;
        [SerializeField] [Min(0f)] private float cooldown;
        [SerializeField] private RangeInt strength = new(80, 100);
        [SerializeField] [Min(.1f)] private float travelSpeed = 50f;
        [SerializeField] [Min(.1f)] private float accelerationTime = .5f;
        [SerializeField] private Ease accelerationEase = Ease.InExpo;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private float explosionRange = 5f;
        [SerializeReference] private IAbilityEffect[] onLaunchEffects = Array.Empty<IAbilityEffect>();

        public AbilityModuleBehaviour CreateModuleBehaviour()
        {
            return new LaunchRocketAbilityModuleBehaviour(new LaunchRocketContext
            {
                RocketPool = rocketPool,
                ExplosionPool = explosionPool,
                Cooldown = cooldown,
                Strength = strength,
                TravelSpeed = travelSpeed,
                AccelerationTime = accelerationTime,
                AccelerationEase = accelerationEase,
                Lifetime = lifetime,
                ExplosionRange = explosionRange,
                OnLaunchEffects = onLaunchEffects
            });
        }
    }

    public struct LaunchRocketContext
    {
        public ScriptablePool RocketPool { get; set; }
        public ScriptablePool ExplosionPool { get; set; }
        public float Cooldown { get; set; }
        public RangeInt Strength { get; set; }
        public float TravelSpeed { get; set; }
        public float AccelerationTime { get; set; }
        public Ease AccelerationEase { get; set; }
        public float Lifetime { get; set; }
        public float ExplosionRange { get; set; }
        public IAbilityEffect[] OnLaunchEffects { get; set; }
    }

    public class LaunchRocketAbilityModuleBehaviour : AbilityModuleBehaviour
    {
        private readonly ScriptablePool _rocketPool;
        private readonly ScriptablePool _explosionPool;
        private readonly float _cooldownDuration;
        private readonly RangeInt _strength;
        private readonly float _travelSpeed;
        private readonly float _accelerationTime;
        private readonly Ease _accelerationEase;
        private readonly float _lifetime;
        private readonly float _explosionRange;
        private readonly AbilityEffectBehaviour[] _onLaunchBehaviours;

        private float _cooldown;

        public LaunchRocketAbilityModuleBehaviour(LaunchRocketContext context)
        {
            _rocketPool = context.RocketPool;
            _explosionPool = context.ExplosionPool;
            _cooldownDuration = context.Cooldown;
            _strength = context.Strength;
            _travelSpeed = context.TravelSpeed;
            _accelerationTime = context.AccelerationTime;
            _accelerationEase = context.AccelerationEase;
            _lifetime = context.Lifetime;
            _explosionRange = context.ExplosionRange;
            _onLaunchBehaviours = (context.OnLaunchEffects ?? Array.Empty<IAbilityEffect>())
                .Select(x => x.CreateEffectBehaviour())
                .ToArray();
        }

        public override void Initialize(RuntimeAbility runtimeAbility)
        {
            base.Initialize(runtimeAbility);
            _cooldown = 0f;

            // Pre-warm pools on equip to avoid instantiation spikes on first launch.
            _rocketPool.Initialize();
            _explosionPool.Initialize();

            foreach (var e in _onLaunchBehaviours)
            {
                e.Initialize(runtimeAbility);
            }
        }

        public override void Tick()
        {
            if (_cooldown > 0f)
            {
                _cooldown -= Time.deltaTime;
            }
        }

        public override void OnInputPressed()
        {
            if (_cooldown > 0f)
            {
                return;
            }

            LaunchRocket();

            foreach (var e in _onLaunchBehaviours)
            {
                e.Trigger();
            }
        }

        private void LaunchRocket()
        {
            var rocket = _rocketPool.RequestObjectAs<PooledRocket>();
            rocket.transform.SetPositionAndRotation(ability.AbilityOrigin.position, Quaternion.FromToRotation(Vector3.right, ability.AimDirection));
            rocket.Initialize(new RocketContext
            {
                BaseDamage = _strength.GetRandom(),
                TravelSpeed = _travelSpeed,
                AccelerationTime = _accelerationTime,
                AccelerationEase = _accelerationEase,
                Lifetime = _lifetime,
                ExplosionPool = _explosionPool,
                ExplosionRange = _explosionRange,
                OwnerStats = ability.Owner.Stats
            });

            _cooldown = _cooldownDuration;
        }
    }
}

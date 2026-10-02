using System;
using CCLBStudio.ScriptablePooling;
using UnityEngine;

namespace Game.ModularWeapon
{
    [Serializable]
    public class SpawnMuzzleWeaponEffect : IWeaponEffect
    {
        [SerializeField] private ScriptablePool muzzlePool;

        public WeaponEffectBehaviour CreateEffectBehaviour()
        {
            return new SpawnMuzzleWeaponEffectBehaviour(muzzlePool);
        }
    }
    
    public class SpawnMuzzleWeaponEffectBehaviour : WeaponEffectBehaviour
    {
        private readonly ScriptablePool _muzzlePool;

        public SpawnMuzzleWeaponEffectBehaviour(ScriptablePool muzzlePool)
        {
            _muzzlePool = muzzlePool;
        }

        public override void Trigger()
        {
            var muzzle = _muzzlePool.RequestObjectAs<AutoReleasePooledObject>();
            var t = muzzle.transform;
            t.SetParent(weapon.MuzzleOrigin);
            t.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        }
    }
}
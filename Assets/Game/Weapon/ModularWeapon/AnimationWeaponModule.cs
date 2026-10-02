using System;
using UnityEngine;

namespace Game.ModularWeapon
{
    [Serializable]
    public class AnimationWeaponModule : IWeaponModule
    {
        [SerializeField] private string shootingTriggerName = "Shooting";
        public WeaponModuleBehaviour CreateModuleBehaviour()
        {
            return new AnimationWeaponModuleBehaviour(shootingTriggerName);
        }
    }
    
    internal class AnimationWeaponModuleBehaviour : WeaponModuleBehaviour
    {
        private readonly int _hash;
        public AnimationWeaponModuleBehaviour(string shootingTriggerName)
        {
            _hash = Animator.StringToHash(shootingTriggerName);
        }
        
        public override void OnStartShooting()
        {
            weapon.Animator.SetBool(_hash, true);
        }
        
        public override void OnStopShooting()
        {
            weapon.Animator.SetBool(_hash, false);
        }
    }
}

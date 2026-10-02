using System;
using CCLBStudio.ScriptablePooling;
using CCLBStudio.ScriptableValue;
using CCLBStudio.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.ModularWeapon
{
    [Serializable]
    public class SpawnCasingEffect : IWeaponEffect
    {
        [SerializeField] private ScriptablePool casingPool;
        [SerializeField] private FloatValue ejectionForce;
        [SerializeField] private FloatValue casingDispersion;
        [SerializeField] private Vector3 ejectionDirection = new Vector3(-1f, 2f, 0f).normalized;
        
        public WeaponEffectBehaviour CreateEffectBehaviour()
        {
            return new SpawnCasingEffectBehaviour(new SpawnCasingContext
            {
                CasingPool = casingPool,
                EjectionDirection = ejectionDirection,
                EjectionForce = ejectionForce,
                CasingDispersion = casingDispersion
            });
        }
    }
    
    public struct SpawnCasingContext
    {
        public ScriptablePool CasingPool { get; set; }
        public Vector3 EjectionDirection { get; set; }
        public FloatValue EjectionForce { get; set; }
        public FloatValue CasingDispersion { get; set; }
    }
    
    public class SpawnCasingEffectBehaviour : WeaponEffectBehaviour
    {
        private readonly ScriptablePool _casingPool;
        private readonly Vector3 _casingEjectionDirection;
        private readonly FloatValue _casingEjectionForce;
        private readonly FloatValue _casingDispersion;
        
        public SpawnCasingEffectBehaviour(SpawnCasingContext context)
        {
            _casingPool = context.CasingPool;
            _casingEjectionDirection = context.EjectionDirection;
            _casingEjectionForce = context.EjectionForce;
            _casingDispersion = context.CasingDispersion;
        }
        
        public override void Trigger()
        {
            var casing = _casingPool.RequestObjectAs<PooledCasing>();
            casing.transform.SetPositionAndRotation(weapon.CasingOrigin.position, Random.rotation);
            Vector3 dir = _casingEjectionDirection.ApplyRotation(Random.Range(-_casingDispersion.Value, _casingDispersion.Value)).normalized;

            casing.Rigidbody.AddForce(dir * _casingEjectionForce.Value, ForceMode.Impulse);
            casing.Rigidbody.AddTorque(VectorExtender.Random01().normalized * _casingEjectionForce.Value, ForceMode.Impulse);
        }
    }
}
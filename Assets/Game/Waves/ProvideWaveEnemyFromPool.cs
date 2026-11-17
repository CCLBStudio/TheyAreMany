using System;
using CCLBStudio.ScriptablePooling;
using CCLBStudio.WaveSystem;
using UnityEngine;

namespace Game.Waves
{
    [CreateAssetMenu(menuName = "CCLB Studio/Wave System/Providers/From Pool", fileName = "ProvideWaveEnemyFromPool")]
    public sealed class ProvideWaveEnemyFromPool : WaveEnemyProvider
    {
        [SerializeField] private ScriptablePool pool;

        [NonSerialized] private bool _prepared;

        public override void Prepare()
        {
            if (!pool)
            {
                throw new InvalidOperationException($"Provider '{name}' has no ScriptablePool.");
            }

            if (_prepared)
            {
                return;
            }

            pool.Initialize();
            _prepared = true;
        }

        public override IWaveEnemy Provide()
        {
            if (!pool)
            {
                throw new InvalidOperationException($"Provider '{name}' has no ScriptablePool.");
            }

            var enemy = pool.RequestObjectAs<EnemyFacade>();
            if (!enemy)
            {
                throw new InvalidOperationException($"Pool '{pool.name}' of provider '{name}' did not return an {nameof(EnemyFacade)}.");
            }

            return enemy;
        }
    }
}

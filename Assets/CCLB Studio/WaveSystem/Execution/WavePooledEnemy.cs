using System;
using CCLBStudio.ScriptablePooling;
using UnityEngine;

namespace CCLBStudio.WaveSystem
{
    [DisallowMultipleComponent]
    public sealed class WavePooledEnemy : MonoBehaviour, IScriptablePooledObject
    {
        public ScriptablePool Pool { get; set; }
        public bool IsSpawned { get; private set; }
        public WaveEnemyDefinition Definition { get; private set; }
        public int WaveNumber { get; private set; }

        public event Action<WavePooledEnemy> Spawned;
        public event Action<WavePooledEnemy> Released;

        public void Despawn()
        {
            if (!IsSpawned || Pool == null)
                throw new InvalidOperationException($"Enemy '{name}' is not leased from a ScriptablePool.");
            Pool.ReleaseObject(this);
            // ScriptablePool.ReleaseObject does not invoke OnObjectReleased, so notify here (idempotent).
            OnObjectReleased();
        }

        public void OnObjectCreated()
        {
            IsSpawned = false;
            Definition = null;
            WaveNumber = 0;
        }

        public void OnObjectRequested()
        {
            if (IsSpawned)
                throw new InvalidOperationException($"Pool requested enemy '{name}' while it was already in use.");
            IsSpawned = true;
        }

        public void OnObjectReleased()
        {
            if (!IsSpawned)
                return;
            IsSpawned = false;
            Definition = null;
            WaveNumber = 0;
            Released?.Invoke(this);
        }

        internal void BindToWave(WaveEnemyDefinition definition, int waveNumber)
        {
            if (!IsSpawned)
                throw new InvalidOperationException($"Pool did not call OnObjectRequested for enemy '{name}'.");
            Definition = definition;
            WaveNumber = waveNumber;
            Spawned?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (!IsSpawned)
                return;
            Debug.LogError($"Pooled wave enemy '{name}' was destroyed instead of being returned to its ScriptablePool.", this);
            OnObjectReleased();
        }
    }
}

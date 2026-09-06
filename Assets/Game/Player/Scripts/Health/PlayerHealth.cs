using CCLBStudio.ScriptableValue;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Player.Scripts.Health
{
    public class PlayerHealth : MonoBehaviour, IPlayerBehaviour
    {
        public PlayerFacade Facade { get; set; }

        [SerializeField] private FloatValue playerHealth;
        [SerializeField] private UnityEvent<IDamageSource, PlayerHealth> onDamagesTaken;
        [SerializeField] private UnityEvent<IDamageSource> onDeath;

        private float _maxHealth;
        private bool _isDead;

        public void Initialize()
        {
            _maxHealth = playerHealth.Value;
        }

        private void TakeDamages(IDamageSource source)
        {
            if(_isDead)
            {
                return;
            }
            
            playerHealth.Value = Mathf.Max(playerHealth.Value - source.GetDamages(), 0);
            
            if (CheckDeath())
            {
                TriggerDeath();
            }
            
            TriggerDamagesTaken(source);
        }

        private void Heal(float amount)
        {
            playerHealth.Value = Mathf.Min(playerHealth.Value + amount, _maxHealth);
        }
        
        private bool CheckDeath()
        {
            return playerHealth.Value <= 0 && !_isDead;
        }

        private void TriggerDeath()
        {
            _isDead = true;
            onDeath?.Invoke(new DebugDamageSource(Vector3.zero, DamageType.Slash, 0));
        }
        
        private void TriggerDamagesTaken(IDamageSource source)
        {
            onDamagesTaken?.Invoke(source, this);
        }

        #region Editor Methods
        #if UNITY_EDITOR

        [Button("Take Damages")]
        private void TakeDamagesEditor(float amount)
        {
            if (!CanCallEditorMethod())
            {
                return;
            }
            
            TakeDamages(new DebugDamageSource(Vector3.zero, DamageType.Slash, amount));
        }
        
        [Button("Heal")]
        private void HealEditor(float amount)
        {
            if (!CanCallEditorMethod())
            {
                return;
            }
            
            Heal(amount);
        }

        private bool CanCallEditorMethod()
        {
            if (!Application.isPlaying)
            {
                Debug.LogError("Can't call this method while not in playmode.");
                return false;
            }

            return true;
        }
        
        #endif
        #endregion
    }
}

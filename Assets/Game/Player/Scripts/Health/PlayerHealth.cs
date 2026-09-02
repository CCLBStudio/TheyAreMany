using System;
using CCLBStudio.ScriptableValue;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Player.Scripts.Health
{
    public class PlayerHealth : MonoBehaviour, IPlayerBehaviour
    {
        public PlayerFacade Facade { get; set; }

        [SerializeField] private FloatValue playerHealth;

        private float _maxHealth;
        private bool _isDead;

        private void Awake()
        {
            _maxHealth = playerHealth.Value;
        }

        private void TakeDamages(float amount)
        {
            playerHealth.Value = Mathf.Max(playerHealth.Value - amount, 0);
            if (playerHealth.Value <= 0)
            {
                _isDead = true;
            }
        }

        private void Heal(float amount)
        {
            playerHealth.Value = Mathf.Min(playerHealth.Value + amount, _maxHealth);
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
            
            TakeDamages(amount);
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

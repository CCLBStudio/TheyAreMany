using System;
using CCLBStudio.ScriptableValue;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Player.Scripts.Health
{
    public class PlayerHealthUI : MonoBehaviour
    {
        [SerializeField] private FloatValue playerHealth;
        [SerializeField] private Image healthBar;

        private float _maxHealth;
        
        void Start()
        {
            _maxHealth = playerHealth.Value;
            playerHealth.OnValueChanged -= OnHealthChanged;
            playerHealth.OnValueChanged += OnHealthChanged;
        }

        private void OnHealthChanged(float newHealth)
        {
            healthBar.fillAmount = newHealth / _maxHealth;
        }
    }
}

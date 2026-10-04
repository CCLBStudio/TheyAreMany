using CCLBStudio.ScreenView;
using CCLBStudio.ScriptableValue;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Player.Health
{
    public class PlayerHealthPanel : ScreenView
    {
        [SerializeField] private FloatValue playerHealth;
        
        [BoxGroup("Health Sliders")]
        [SerializeField] private Image healthBarMain;
        [BoxGroup("Health Sliders")]
        [SerializeField] private Image healthBarSecondary;

        private HealthFillUI _fillUI;
        
        void Start()
        {
            _fillUI = new HealthFillUI(healthBarMain, healthBarSecondary, playerHealth.Value);
            
            playerHealth.OnValueChanged -= OnHealthChanged;
            playerHealth.OnValueChanged += OnHealthChanged;
        }

        private void OnHealthChanged(float newHealth)
        {
            _fillUI?.AnimateTo(newHealth);
        }
    }
}

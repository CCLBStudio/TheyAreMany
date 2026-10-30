using CCLBStudio.ScreenView;
using Game.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Player.Health
{
    public class PlayerHealthPanel : ScreenView
    {
        [SerializeField] private HealthValue playerHealth;
        
        [BoxGroup("Health Sliders")]
        [SerializeField] private Image healthBarMain;
        [BoxGroup("Health Sliders")]
        [SerializeField] private Image healthBarSecondary;

        private HealthFillUI _fillUI;
        
        protected override void Start()
        {
            base.Start();

            healthBarMain.fillAmount = playerHealth.NormalizedValue;
            healthBarSecondary.fillAmount = playerHealth.NormalizedValue;
            _fillUI = new HealthFillUI(healthBarMain, healthBarSecondary, playerHealth.MaxValue);
            
            playerHealth.OnValueChanged -= OnHealthChanged;
            playerHealth.OnValueChanged += OnHealthChanged;
        }

        private void OnHealthChanged(float newHealth)
        {
            _fillUI?.AnimateTo(newHealth);
        }
    }
}

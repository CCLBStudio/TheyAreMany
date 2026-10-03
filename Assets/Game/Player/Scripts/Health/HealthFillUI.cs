using PrimeTween;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Player.Scripts.Health
{
    public class HealthFillUI
    {
        private Image _main;
        private Image _secondary;
        private float _maxHealth;
        private float _currentHealth;
        private Sequence _currentSequence;
        
        public HealthFillUI(Image main, Image secondary, float maxHealth)
        {
            _main = main;
            _secondary = secondary;
            _maxHealth = maxHealth;
            _currentHealth = maxHealth;
        }

        public void AnimateTo(float newHealth)
        {
            if (Mathf.Approximately(newHealth, _currentHealth))
            {
                return;
            }
            
            if (newHealth > _currentHealth)
            {
                HealAnimation(newHealth / _maxHealth);
            }
            else
            {
                DamageReceivedAnimation(newHealth / _maxHealth);
            }

            _currentHealth = newHealth;
        }

        private void DamageReceivedAnimation(float normalizedHealth)
        {
            if (_currentSequence.isAlive)
            {
                _currentSequence.Stop();
            }
            
            _currentSequence = Sequence.Create()
                .Group(Tween.UIFillAmount(_secondary, normalizedHealth, 1f, Ease.OutQuart, startDelay: .6f))
                .Group(Tween.UIFillAmount(_main, normalizedHealth, .1f, Ease.OutExpo));
        }

        private void HealAnimation(float normalizedHealth)
        {
            if (_currentSequence.isAlive)
            {
                _currentSequence.Stop();
            }
            
            _currentSequence = Sequence.Create()
                .Group(Tween.UIFillAmount(_secondary, normalizedHealth, .5f, Ease.InOutSine))
                .Group(Tween.UIFillAmount(_main, normalizedHealth, .5f, Ease.InOutSine));
        }
    }
}
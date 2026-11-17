using System;
using Game.Damage;
using Game.Stats;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Enemies
{
    public class EnemyHealth : MonoBehaviour, IEnemyBehaviour, IHealth, IResistanceProvider
    {
        public EnemyFacade Facade { get; set; }
        public event Action<IDamageContext> OnDeath;
        public bool IsMaxHealth => _currentHealth >= Facade.EnemyData.MaxHealth;
        public float CurrentHealth => _currentHealth;

        [SerializeField] private UnityEvent<IDamageSource> onDeath;
        [SerializeField] private UnityEvent<IDamageContext, Vector3?> onDamageTaken;

        private int _currentHealth;
        private ICharacterStats _stats;
    
        public void OnEnemyCreated()
        {
            if(TryGetComponent(out ICharacterStats stats))
            {
                _stats = stats;
            }
            else
            {
                Debug.LogWarning("EnemyHealth: No ICharacterStats component found. Using default stats.");
                _stats = new DefaultCharacterStats();
            }
            
            OnDeath += context => onDeath?.Invoke(context.Source);
        }

        public void OnEnemyRequested()
        {
            _currentHealth = Facade.EnemyData.MaxHealth;
        }

        public void OnEnemyReleased()
        {
        }

        public ResistanceDescriptor GetResistances(DamageType damageType)
        {
            return _stats.GetResistances(damageType);
        }

        public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint = null)
        {
            if (_currentHealth <= 0f)
            {
                return;
            }
        
            _currentHealth -= damageContext.Damage.FinalDamage;
            if (_currentHealth <= 0f)
            {
                OnDeath?.Invoke(damageContext);
            }
            
            onDamageTaken?.Invoke(damageContext, hitPoint);
        }

        public void Heal(float amount)
        {
            
        }

    }
}

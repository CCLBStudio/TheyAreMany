using CCLBStudio.EventBus;
using CCLBStudio.ScriptableValue;
using Game.Damage;
using UnityEngine;

namespace Game.Objective
{
    public class MainObjectiveHealth : MonoBehaviour, IHealth
    {
        [SerializeField] private FloatValue objectiveHealth;
        
        private bool _isDead;
        private float _maxHealth;

        private void Awake()
        {
            _maxHealth = objectiveHealth.Value;
        }

        public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint = null)
        {
            if(_isDead)
            {
                return;
            }
            
            objectiveHealth.Value = Mathf.Max(objectiveHealth.Value - damageContext.Damage.FinalDamage, 0);
            
            if (CheckDeath())
            {
                TriggerDeath(damageContext);
            }
            
            TriggerDamagesTaken(damageContext);
        }

        public void Heal(float amount)
        {
            objectiveHealth.Value = Mathf.Min(objectiveHealth.Value + amount, _maxHealth);
        }
        
        private void TriggerDeath(IDamageContext ctx)
        {
            _isDead = true;
            EvtBus.Raise(new ObjectiveDestroyed(ctx));
        }
        
        private void TriggerDamagesTaken(IDamageContext damageContext)
        {
            EvtBus.Raise(new ObjectiveDamaged(damageContext));
        }
        
        private bool CheckDeath()
        {
            return objectiveHealth.Value <= 0 && !_isDead;
        }
    }
}
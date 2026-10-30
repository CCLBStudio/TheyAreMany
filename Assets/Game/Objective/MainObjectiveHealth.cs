using CCLBStudio.EventBus;
using CCLBStudio.ScriptableValue;
using Game.Damage;
using Game.Utils;
using UnityEngine;

namespace Game.Objective
{
    public class MainObjectiveHealth : MonoBehaviour, IHealth
    {
        [SerializeField] private HealthValue objectiveHealth;
        
        private bool _isDead;

        public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint = null)
        {
            if(_isDead)
            {
                return;
            }
            
            objectiveHealth.Decrement(damageContext.Damage.FinalDamage);
            
            if (CheckDeath())
            {
                TriggerDeath(damageContext);
            }
            
            TriggerDamagesTaken(damageContext);
        }

        public void Heal(float amount)
        {
            objectiveHealth.Increment(amount);
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
            return objectiveHealth.IsZero() && !_isDead;
        }
    }
}
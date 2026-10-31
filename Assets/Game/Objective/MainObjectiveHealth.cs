using CCLBStudio.EventBus;
using Game.Damage;
using Game.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Objective
{
    public class MainObjectiveHealth : MonoBehaviour, IHealth
    {
        [SerializeField] private HealthValue objectiveHealth;
        [SerializeField] private UnityEvent<IDamageContext, Vector3?> onDamagesTaken;
        
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
            
            TriggerDamagesTaken(damageContext, hitPoint);
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
        
        private void TriggerDamagesTaken(IDamageContext damageContext, Vector3? hitPoint)
        {
            EvtBus.Raise(new ObjectiveDamaged(damageContext));
            onDamagesTaken?.Invoke(damageContext, hitPoint);
        }
        
        private bool CheckDeath()
        {
            return objectiveHealth.IsZero() && !_isDead;
        }
    }
}
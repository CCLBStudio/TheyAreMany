using CCLBStudio.EventBus;
using CCLBStudio.ScriptableValue;
using Game.Damage;
using Game.Stats;
using Game.Utils;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Player.Health
{
    public class PlayerHealth : MonoBehaviour, IPlayerBehaviour, IHealth, IResistanceProvider
    {
        public PlayerFacade Facade { get; set; }
        public bool IsAlive => !_isDead;

        [SerializeField] private HealthValue playerHealth;
        [SerializeField] private UnityEvent<IDamageContext, PlayerHealth> onDamagesTaken;

        private bool _isDead;
        private ICharacterStats _stats;

        public void Initialize()
        {
            if(TryGetComponent(out ICharacterStats stats))
            {
                _stats = stats;
            }
            else
            {
                Debug.LogWarning("PlayerHealth: No ICharacterStats component found on the player. Using default stats.");
                _stats = new DefaultCharacterStats();
            }
        }

        public ResistanceDescriptor GetResistances(DamageType damageType)
        {
            return _stats.GetResistances(damageType);
        }

        public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint = null)
        {
            if(_isDead)
            {
                return;
            }
            
            playerHealth.Decrement(damageContext.Damage.FinalDamage);
            
            if (CheckDeath())
            {
                TriggerDeath(damageContext.Source);
            }
            
            TriggerDamagesTaken(damageContext);
        }

        public void Heal(float amount)
        {
            playerHealth.Increment(amount);
        }
        
        private bool CheckDeath()
        {
            return playerHealth.IsZero() && !_isDead;
        }

        private void TriggerDeath(IDamageSource source)
        {
            _isDead = true;
            EvtBus.Raise(new PlayerKilled(Facade, source));
        }
        
        private void TriggerDamagesTaken(IDamageContext damageContext)
        {
            onDamagesTaken?.Invoke(damageContext, this);
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
            
            var ctx = new DamageContext(new DebugDamageSource(Vector3.zero, DamageType.Slashing), new ComputedDamage
            {
                TrueDamage = (int)amount,
                ReducedDamage = (int)amount,
                ReducedAmount = 0,
                FinalDamage = (int)amount
            });
            ReceiveDamages(ctx);
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

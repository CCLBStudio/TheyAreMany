using CCLBStudio.EventBus;
using CCLBStudio.ScriptableValue;
using Game.Damage;
using Game.Stats;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Game.Player.Health
{
    public class PlayerHealth : MonoBehaviour, IPlayerBehaviour, IHealth
    {
        public PlayerFacade Facade { get; set; }

        [SerializeField] private FloatValue playerHealth;
        [SerializeField] private UnityEvent<IDamageContext, PlayerHealth> onDamagesTaken;

        private float _maxHealth;
        private bool _isDead;
        private ICharacterStats _stats;

        public void Initialize()
        {
            _maxHealth = playerHealth.Value;
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
            
            playerHealth.Value = Mathf.Max(playerHealth.Value - damageContext.Damage.FinalDamage, 0);
            
            if (CheckDeath())
            {
                TriggerDeath(damageContext.Source);
            }
            
            TriggerDamagesTaken(damageContext);
        }

        public void Heal(float amount)
        {
            playerHealth.Value = Mathf.Min(playerHealth.Value + amount, _maxHealth);
        }
        
        private bool CheckDeath()
        {
            return playerHealth.Value <= 0 && !_isDead;
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

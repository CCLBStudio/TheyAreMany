using CCLBStudio.Extensions;
using Game.Damage;
using Game.Stats;
using UnityEngine;

namespace Game.Player
{
    public class AttackReceiver : MonoBehaviour, IDamageTarget
    {
        private IHealth _health;
        private ICharacterStats _stats;

        private void Start()
        {
            _health = this.GetComponentFromRoot<IHealth>();
            _stats = this.GetComponentFromRoot<ICharacterStats>();
            
            if (_health == null)
            {
                Debug.LogError("No health found.");
                Destroy(this);
            }
            if (_stats == null)
            {
                Debug.LogWarning("No character stats found.");
                _stats = new DefaultCharacterStats();
            }
        }

        public ResistanceDescriptor GetResistances(DamageType damageType)
        {
            return _stats.GetResistances(damageType);
        }

        public void ReceiveDamages(IDamageContext damageContext, Vector3? hitPoint = null)
        {
            _health.ReceiveDamages(damageContext, hitPoint);
        }
    }
}

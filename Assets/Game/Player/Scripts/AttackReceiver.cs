using CCLBStudio.Extensions;
using UnityEngine;

namespace Game.Player
{
    public class AttackReceiver : MonoBehaviour, IDamageTarget
    {
        private IHealth _health;

        private void Start()
        {
            _health = this.GetComponentFromRoot<IHealth>();
            if (_health == null)
            {
                Debug.LogError("No health found.");
                Destroy(this);
            }
        }

        public void ReceiveDamages(IDamageSource damageSource)
        {
            _health.ReceiveDamages(damageSource);
        }
    }
}

using CCLBStudio.Extensions;
using Game.Player.Scripts.Health;
using UnityEngine;

namespace Game.Player
{
    public class AttackReceiver3D : MonoBehaviour, IDamageTarget
    {
        private PlayerHealth _health;

        private void Start()
        {
            _health = this.GetComponentFromRoot<PlayerHealth>();
            if (!_health)
            {
                Debug.LogError("No PlayerHealth found.");
                Destroy(this);
            }
        }

        public void ReceiveDamages(IDamageSource damageSource)
        {
            _health.TakeDamages(damageSource);
        }
    }
}

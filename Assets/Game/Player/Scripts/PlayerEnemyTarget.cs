using CCLBStudio.DependencyInjection;
using Game.Enemies;
using Game.Player.Health;
using UnityEngine;

namespace Game.Player
{
    public partial class PlayerEnemyTarget : MonoBehaviour, IPlayerBehaviour, IEnemyTarget
    {
        public PlayerFacade Facade { get; set; }
        public int TargetPriority => 0;
        public Transform TargetTransform => transform;

        private PlayerHealth _health;
        [Inject] private EnemyTargetList _enemyTargets;
        
        public void Initialize()
        {
            _health = Facade.GetBehaviour<PlayerHealth>();
            if (!_health)
            {
                Debug.LogError("No health found.");
                Destroy(this);
                return;
            }

            _enemyTargets.targets.Add(this);
        }

        public bool CanBeSelected()
        {
            return _health.IsAlive;
        }
    }
}
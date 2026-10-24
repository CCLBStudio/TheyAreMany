using CCLBStudio.DependencyInjection;
using Game.Enemies;
using UnityEngine;

namespace Game.Objective
{
    public partial class EnemyObjective : MonoBehaviour, IEnemyTarget
    {
        public int TargetPriority => targetPriority;
        public Transform TargetTransform => transform;

        [SerializeField] private int targetPriority = 10;
        
        [Inject] private EnemyTargetList _enemyTargetList;

        private void Awake()
        {
            _enemyTargetList.targets.Add(this);
        }
        
        public bool CanBeSelected() => true;
    }
}
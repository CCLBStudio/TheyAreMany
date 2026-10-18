using CCLBStudio.DependencyInjection;
using Game.Enemies;
using UnityEngine;

namespace Game.Objective
{
    public partial class MainEnemyObjective : MonoBehaviour, IEnemyTarget
    {
        public int TargetPriority => 10;
        public Transform TargetTransform => transform;
        public bool CanBeSelected() => true;

        [Inject] private EnemyTargetList _enemyTargetList;

        private void Awake()
        {
            _enemyTargetList.targets.Add(this);
        }
    }
}
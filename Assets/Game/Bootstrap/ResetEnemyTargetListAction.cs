using CCLBStudio.DependencyInjection;
using CCLBStudio.ScriptableBehaviours;
using Game.Enemies;
using UnityEngine;

namespace Game.Bootstrap
{
    [CreateAssetMenu(fileName = "ResetEnemyTargetListAction", menuName = "They Are Many/Actions/ResetEnemyTargetList")]
    public partial class ResetEnemyTargetListAction : ScriptableAction
    {
        [Inject] private EnemyTargetList enemyTargetList;
        public override void Execute()
        {
            enemyTargetList.targets.Clear();
        }
    }
}
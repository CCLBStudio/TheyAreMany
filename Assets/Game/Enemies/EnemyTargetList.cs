using System.Collections.Generic;
using CCLBStudio.DependencyInjection;

namespace Game.Enemies
{
    [Provide]
    public partial class EnemyTargetList
    {
        public readonly List<IEnemyTarget> targets = new();
    }
}
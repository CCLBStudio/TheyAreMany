using System.Collections.Generic;
using CCLBStudio.Bootstrapping;
using CCLBStudio.DependencyInjection;
using CCLBStudio.ScriptableBehaviours;
using UnityEngine;

namespace Game.Enemies
{
    [Provide]
    public partial class EnemyTargetList
    {
        public readonly List<IEnemyTarget> targets = new();
    }
}
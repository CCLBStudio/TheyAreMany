using UnityEngine;

namespace Game.Enemies
{
    public interface IEnemyTarget
    {
        public int TargetPriority { get; }
        public Transform TargetTransform { get; }
        public bool CanBeSelected();
    }
}
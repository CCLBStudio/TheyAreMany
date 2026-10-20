using System;
using CCLBStudio.DependencyInjection;
using UnityEngine;
using ZLinq;

namespace Game.Enemies
{
    [Serializable]
    public abstract class TargetSelectionStrategy
    {
        protected EnemyTargetSelector selector;
        
        public IEnemyTarget CurrentTarget { get; protected set; }
        public abstract void SelectTarget();
        public abstract void Tick();
        
        public virtual void Initialize(EnemyTargetSelector s)
        {
            selector = s;
        }
        
        public void SetTarget(IEnemyTarget t) => CurrentTarget = t;

        protected bool ShouldAcquireNewTarget()
        {
            return CurrentTarget == null || !CurrentTarget.CanBeSelected();
        }
    }

    [Serializable]
    public class SelectMostImportantTarget : TargetSelectionStrategy
    {
        public override void SelectTarget()
        {
            CurrentTarget = selector.Targets
                .AsValueEnumerable()
                .Where(t => t.CanBeSelected())
                .OrderByDescending(t => t.TargetPriority)
                .FirstOrDefault();
        }

        public override void Tick()
        {
            if (ShouldAcquireNewTarget())
            {
                SelectTarget();
            }
        }
    }
    
    [Serializable]
    public class SelectClosestTarget : TargetSelectionStrategy
    {
        public override void SelectTarget()
        {
            CurrentTarget = selector.Targets
                .AsValueEnumerable()
                .Where(t => t.CanBeSelected())
                .OrderBy(t => Vector3.Distance(selector.transform.position, t.TargetTransform.position))
                .FirstOrDefault();
        }

        public override void Tick()
        {
            SelectTarget();
        }
    }
}
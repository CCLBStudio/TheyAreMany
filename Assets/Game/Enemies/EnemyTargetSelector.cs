using System.Collections.Generic;
using System.Linq;
using CCLBStudio.DependencyInjection;
using CCLBStudio.Extensions;
using CCLBStudio.GlobalUpdater;
using UnityEngine;

namespace Game.Enemies
{
    public partial class EnemyTargetSelector : InjectedMonoBehaviour, IFixedUpdate, IEnemyBehaviour
    {
        public EnemyFacade Facade { get; set; }
        public Transform Target => targetSelection?.CurrentTarget?.TargetTransform;
        public List<IEnemyTarget> Targets => _targets.targets;

        [SerializeReference] private TargetSelectionStrategy targetSelection;
        
        [Inject] private EnemyTargetList _targets;
        private readonly List<IEnemyTarget> _collidingTargets = new List<IEnemyTarget>();

        public void FixedTick()
        {
            targetSelection.Tick();
        }
        
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!collision.collider.TryGetComponentOnRoot(out IEnemyTarget target))
            {
                return;
            }
            
            _collidingTargets.Add(target);

            if(target.CanBeSelected() && _collidingTargets.Count(t => t.CanBeSelected()) <= 1)
            {
                targetSelection.SetTarget(target);
            }
        }

        private void OnCollisionExit2D(Collision2D other)
        {
            if (!other.collider.TryGetComponentOnRoot(out IEnemyTarget target))
            {
                return;
            }
            
            _collidingTargets.Remove(target);
            if (_collidingTargets.Count(t => t.CanBeSelected()) > 0)
            {
                targetSelection.SetTarget(_collidingTargets.First(t => t.CanBeSelected()));
            }
            else
            {
                targetSelection.SelectTarget();
            }
        }

        public void OnEnemyCreated()
        {
            targetSelection.Initialize(this);
        }

        public void OnEnemyRequested()
        {
            GlobalUpdater.RegisterFixedUpdate(this);
        }

        public void OnEnemyReleased()
        {
            GlobalUpdater.UnregisterFixedUpdate(this);
        }

        public bool AutoRegisterToGlobalUpdater() => false;
    }
}

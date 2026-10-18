using System.Collections.Generic;
using CCLBStudio.DependencyInjection;
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

        public void FixedTick()
        {
            targetSelection.Tick();
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

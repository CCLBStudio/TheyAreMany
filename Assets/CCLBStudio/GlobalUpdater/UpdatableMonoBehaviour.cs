using UnityEngine;

namespace CCLBStudio.GlobalUpdater
{
    public class UpdatableMonoBehaviour : MonoBehaviour, IUpdatable, IFixedUpdatable, ILateUpdatable
    {
        public GlobalUpdateType UpdateType => updateType;
        
        [SerializeField] private GlobalUpdateType updateType = GlobalUpdateType.Update;

        protected virtual void OnEnable()
        {
            GlobalUpdater.RegisterUpdatableMonoBehaviour(this);
        }

        protected void OnDisable()
        {
            //GlobalUpdater.UnregisterUpdatableMonoBehaviour(this);
        }

        public virtual void Tick()
        {
        }

        public virtual void FixedTick()
        {
        }

        public virtual void LateTick()
        {
        }
    }
}

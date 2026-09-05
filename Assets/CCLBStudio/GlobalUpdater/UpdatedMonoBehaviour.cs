using UnityEngine;

namespace CCLBStudio.GlobalUpdater
{
    public class UpdatedMonoBehaviour : MonoBehaviour, IUpdate, IFixedUpdate, ILateUpdate
    {
        public GlobalUpdateType UpdateType => updateType;
        
        [SerializeField] private GlobalUpdateType updateType = GlobalUpdateType.Update;

        protected virtual void OnEnable()
        {
            GlobalUpdater.RegisterUpdatedMonoBehaviour(this);
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

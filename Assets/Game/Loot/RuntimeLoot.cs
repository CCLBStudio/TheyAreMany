using CCLBStudio.Extensions;
using CCLBStudio.ScriptablePooling;
using UnityEngine;

namespace Game.Loot
{
    public abstract class RuntimeLoot<TRootComponent> : MonoBehaviour, IScriptablePooledObject
    {
        public virtual ScriptablePool Pool { get; set; }

        [SerializeField] private bool destroyOnConsume = true;
        
        private void OnTriggerEnter2D(Collider2D other)
        {            
            if(!other.TryGetComponentOnRoot(out TRootComponent comp))
            {
                return;
            }

            if(!CanConsume(comp))
            {
                return;
            }
            
            Consume(comp);
            
            if(destroyOnConsume)
            {
                Destroy(gameObject);
            }
        }
        
        protected abstract void Consume(TRootComponent component);
        
        protected virtual bool CanConsume(TRootComponent component) => true;
        
        public virtual void OnObjectCreated()
        {
        }

        public virtual void OnObjectRequested()
        {
        }

        public virtual void OnObjectReleased()
        {
        }
    }
}
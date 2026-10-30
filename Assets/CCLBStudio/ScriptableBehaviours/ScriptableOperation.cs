using UnityEngine;

namespace CCLBStudio.ScriptableBehaviours
{
    public abstract class ScriptableOperation<T> : ScriptableObject
    {
        public abstract T Execute();
    }
}

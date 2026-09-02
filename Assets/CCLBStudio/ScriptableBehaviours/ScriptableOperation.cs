using UnityEngine;

namespace Systems.ScriptableBehaviours
{
    public abstract class ScriptableOperation<T> : ScriptableObject
    {
        public abstract T Execute();
    }
}

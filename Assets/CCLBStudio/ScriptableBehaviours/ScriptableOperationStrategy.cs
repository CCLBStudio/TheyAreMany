using UnityEngine;

namespace CCLBStudio.ScriptableBehaviours
{
    public abstract class ScriptableOperationStrategy<T, TR> : ScriptableObject
    {
        public abstract TR Execute(T parameter);
    }
}

using UnityEngine;

namespace Systems.ScriptableBehaviours
{
    public abstract class ScriptableOperationStrategy<T, TR> : ScriptableObject
    {
        public abstract TR Execute(T parameter);
    }
}

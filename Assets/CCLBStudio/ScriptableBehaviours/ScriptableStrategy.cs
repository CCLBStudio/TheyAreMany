using UnityEngine;

namespace Systems.ScriptableBehaviours
{
    public abstract class ScriptableStrategy<T> : ScriptableObject
    {
        public abstract void Execute(T parameter);
    }
}

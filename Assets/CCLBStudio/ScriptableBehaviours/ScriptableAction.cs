using UnityEngine;

namespace Systems.ScriptableBehaviours
{
    public abstract class ScriptableAction : ScriptableObject
    {
        public abstract void Execute();
    }
}

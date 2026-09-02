using UnityEngine;

namespace Systems.ScriptableBehaviours
{
    public abstract class ScriptableCondition : ScriptableObject
    {
        public abstract bool Check();
    }
}

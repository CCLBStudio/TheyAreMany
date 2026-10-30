using UnityEngine;

namespace CCLBStudio.ScriptableBehaviours
{
    public abstract class ScriptableCondition : ScriptableObject
    {
        public abstract bool Check();
    }
}

using System.Collections.Generic;
using CCLBStudio.ScriptableBehaviours;
using CCLBStudio.ScriptableValue;
using UnityEngine;

namespace Game.Tools
{
    [CreateAssetMenu(fileName = "ResetScriptableValue", menuName = "They Are Many/Actions/ResetScriptableValue")]
    public class ResetScriptableValue : ScriptableAction
    {
        [SerializeField] private List<BaseScriptableValue> toReset;
    
        public override void Execute()
        {
            foreach (var item in toReset)
            {
                item.HardReset();
            }
        }
    }
}

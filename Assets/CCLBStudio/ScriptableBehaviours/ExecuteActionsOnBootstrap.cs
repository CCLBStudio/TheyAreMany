using System;
using System.Collections.Generic;
using System.Linq;
using CCLBStudio.Bootstrapping;
using UnityEngine;

namespace CCLBStudio.ScriptableBehaviours
{
    [Serializable]
    public class ExecuteActionsOnBootstrap : IBootstrapped
    {
        [SerializeField] private List<ScriptableAction> actions = new();

        public void Initialize()
        {
            foreach (var action in actions.Where(x => x))
            {
                action.Execute();
            }
        }
    }
}

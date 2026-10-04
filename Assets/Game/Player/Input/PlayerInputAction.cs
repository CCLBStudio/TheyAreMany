using System;
using System.Collections.Generic;
using Systems.ScriptableBehaviours;
using UnityEngine;
using ZLinq;

namespace Game.Player.Input
{
    [CreateAssetMenu(menuName = "They Are Many/Scriptable Behaviours/Actions/Player Input Action", fileName = "NewPlayerInputAction")]
    public class PlayerInputAction : ScriptableAction
    {
        [SerializeField] private List<InputReader> inputs;
        [SerializeField] private EnableAction action;
        [SerializeField] private InputTarget target;
        
        private enum EnableAction {Enable, Disable}
        [Flags] private enum InputTarget {InGame = 1, UI = 2}
        
        public override void Execute()
        {
            foreach (var i in inputs.AsValueEnumerable().Where(x => x))
            {
                if (action == EnableAction.Enable)
                {
                    if (target.HasFlag(InputTarget.InGame))
                    {
                        i.EnableInGameInputs();
                    }

                    if (target.HasFlag(InputTarget.UI))
                    {
                        i.EnableUiInputs();
                    }
                }
                else
                {
                    if (target.HasFlag(InputTarget.InGame))
                    {
                        i.DisableInGameInputs();
                    }

                    if (target.HasFlag(InputTarget.UI))
                    {
                        i.DisableUiInputs();
                    }
                }
            }
        }
    }
}
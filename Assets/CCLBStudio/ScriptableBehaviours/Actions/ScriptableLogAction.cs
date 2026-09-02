using System;
using UnityEngine;

namespace Systems.ScriptableBehaviours.Actions
{
    public class ScriptableLogAction : ScriptableAction
    {
        [SerializeField] private LogLevels logLevel = LogLevels.Verbose;
        [SerializeField] private string logMessage;
        
        private enum LogLevels {Verbose, Warning, Error, Exception}

        public override void Execute()
        {
            switch (logLevel)
            {
                case LogLevels.Verbose:
                    Debug.Log(logMessage);
                    break;
            
                case LogLevels.Warning:
                    Debug.LogWarning(logMessage);
                    break;
                case LogLevels.Error:
                    Debug.LogError(logMessage);
                    break;
            
                case LogLevels.Exception:
                    throw new Exception(logMessage);
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

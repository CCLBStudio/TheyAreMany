using System;
using System.Collections.Generic;
using CCLBStudio.DependencyInjection;
using CCLBStudio.EventBus;
using CCLBStudio.ScriptableValue;
using Game.Player.Health;
using Systems.ScriptableBehaviours;
using UnityEngine;

namespace Game.GameOver
{
    public sealed partial class GameOverManager : MonoBehaviour, IEventListener<PlayerKilled>
    {
        [SerializeField] private List<ScriptableAction> onGameOver;
        
        [Inject] private PlayerFacadeListValue _players;
        [NonSerialized] private int _killedPlayers;
        
        private void OnEnable()
        {
            EvtBus.Subscribe(this);
        }

        private void OnDisable()
        {
            EvtBus.Unsubscribe(this);
        }

        public void OnEvent(in PlayerKilled evt)
        {
            _killedPlayers++;
            if (_killedPlayers >= _players.Count)
            {
                foreach (var action in onGameOver)
                {
                    action.Execute();
                }
            }
        }
    }
}

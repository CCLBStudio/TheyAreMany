using System;
using System.Collections.Generic;
using CCLBStudio.DependencyInjection;
using CCLBStudio.EventBus;
using CCLBStudio.ScriptableValue;
using Game.Objective;
using Game.Player.Health;
using Systems.ScriptableBehaviours;
using UnityEngine;

namespace Game.GameOver
{
    public sealed partial class GameOverManager : MonoBehaviour, IEventListener<PlayerKilled>, IEventListener<ObjectiveDestroyed>
    {
        [SerializeField] private List<ScriptableAction> onGameOver;
        
        [Inject] private PlayerFacadeListValue _players;
        [NonSerialized] private int _killedPlayers;
        private bool _gameOver;
        
        private void OnEnable()
        {
            EventBus<PlayerKilled>.Subscribe(this);
            EventBus<ObjectiveDestroyed>.Subscribe(this);
        }

        private void OnDisable()
        {
            EventBus<PlayerKilled>.Unsubscribe(this);
            EventBus<ObjectiveDestroyed>.Unsubscribe(this);
        }

        public void OnEvent(in PlayerKilled evt)
        {
            _killedPlayers++;
            if (_killedPlayers >= _players.Count)
            {
                TriggerGameOver();
            }
        }
        
        public void OnEvent(in ObjectiveDestroyed evt)
        {
            TriggerGameOver();
        }
        
        private void TriggerGameOver()
        {
            if (_gameOver)
            {
                return;
            }
            
            _gameOver = true;
            foreach (var action in onGameOver)
            {
                action.Execute();
            }
        }
    }
}

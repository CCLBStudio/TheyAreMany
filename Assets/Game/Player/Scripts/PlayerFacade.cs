using System;
using System.Collections.Generic;
using System.Linq;
using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptableValue;
using UnityEngine;

public class PlayerFacade : MonoBehaviour
{
    [SerializeField] private PlayerFacadeListValue players;

    private IPlayerBehaviour[] _behaviours;
    private Dictionary<Type, IPlayerBehaviour> _behaviourDictionary;
    
    private void Start()
    {
        players.Add(this);
        _behaviours = GetComponentsInChildren<IPlayerBehaviour>();
        _behaviourDictionary = _behaviours.ToDictionary(b => b.GetType(), b => b);

        foreach (var b in _behaviours)
        {
            b.Facade = this;
            b.Initialize();
            GlobalUpdater.RegisterUpdatedObject(b);
        }
    }
    
    public T GetBehaviour<T>() where T : IPlayerBehaviour
    {
        if (_behaviourDictionary.TryGetValue(typeof(T), out var behaviour))
        {
            return (T)behaviour;
        }
        
        throw new Exception($"Behaviour of type {typeof(T)} not found.");
    }

    private void OnDestroy()
    {
        foreach (var b in _behaviours)
        {
            GlobalUpdater.UnregisterUpdatedObject(b);
        }
    }
}

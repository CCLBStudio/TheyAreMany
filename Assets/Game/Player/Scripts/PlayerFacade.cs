using System;
using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptableValue;
using UnityEngine;

public class PlayerFacade : MonoBehaviour
{
    [SerializeField] private PlayerFacadeListValue players;

    private IPlayerBehaviour[] _behaviours;
    
    private void Start()
    {
        players.Add(this);
        _behaviours = GetComponentsInChildren<IPlayerBehaviour>();

        foreach (var b in _behaviours)
        {
            b.Facade = this;
            b.Initialize();
            GlobalUpdater.RegisterUpdatedObject(b);
        }
    }

    private void OnDestroy()
    {
        foreach (var b in _behaviours)
        {
            GlobalUpdater.UnregisterUpdatedObject(b);
        }
    }
}

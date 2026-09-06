using System;
using CCLBStudio.GlobalUpdater;
using CCLBStudio.ScriptableValue;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyTargetSelector : MonoBehaviour, IFixedUpdate, IEnemyBehaviour
{
    public EnemyFacade Facade { get; set; }
    public Transform Target { get; set; }

    [SerializeField] private TargetSelection targetSelectionMethod = TargetSelection.Closest;
    [SerializeField] protected PlayerFacadeListValue players;
    private enum TargetSelection {Closest, Random}

    private Action _targetSelectionMethod;

    public void FixedTick()
    {
        _targetSelectionMethod?.Invoke();
    }

    private void ChooseTargetSelectionMethod()
    {
        switch (targetSelectionMethod)
        {
            case TargetSelection.Closest:
                _targetSelectionMethod = SelectClosestTarget;
                break;
            
            case TargetSelection.Random:
                _targetSelectionMethod = SelectRandomTarget;
                break;
        }
    }

    private void SelectRandomTarget()
    {
        Target = players.Value[Random.Range(0, players.Value.Count)].transform;
    }

    private void SelectClosestTarget()
    {
        float distance = float.MaxValue;

        foreach (var p in players.Value)
        {
            float d = Vector3.Distance(p.transform.position, transform.position);
            if (d < distance)
            {
                distance = d;
                Target = p.transform;
            }
        }
    }

    public void OnEnemyCreated()
    {
    }

    public void OnEnemyRequested()
    {
        ChooseTargetSelectionMethod();
        _targetSelectionMethod?.Invoke();

        if (targetSelectionMethod == TargetSelection.Closest)
        {
            GlobalUpdater.RegisterFixedUpdate(this);
        }
    }

    public void OnEnemyReleased()
    {
        GlobalUpdater.UnregisterFixedUpdate(this);
    }

    public bool AutoRegisterToGlobalUpdater() => false;
}

using System.Collections.Generic;
using Game.Transition;
using Sirenix.OdinInspector;
using TransitionsPlus;
using UnityEngine;

public class DebugAndTest : MonoBehaviour
{
    public TransitionProfile profile;
    public TransitionId transitionId;
    public List<TransitionId> transitionIds = new();
    [SerializeField] private Dictionary<TransitionId, TransitionProfile> profiles = new();

    [Button("Start Transition")]
    public void StartTransition()
    {
        TransitionAnimator.Start(profile);
    }
}

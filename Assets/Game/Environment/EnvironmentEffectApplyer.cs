using System;
using System.Collections.Generic;
using System.Linq;
using CCLBStudio.EventBus;
using CCLBStudio.SceneLoading;
using PrimeTween;
using UnityEngine;
using ZLinq;

public class EnvironmentEffectApplyer : MonoBehaviour, IEventListener<WillUnloadScene>
{
    [SerializeField] private LevelGenerator level;
    [SerializeField] private Vector3[] rotationAxis = new[] { Vector3.up, Vector3.down, Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
    
    private int _currentAxis;
    private readonly Dictionary<Transform, Queue<QueuedTweenInfo>> _queuedTweens = new();
    private readonly Dictionary<Transform, Tween> _currentlyTweening = new();

    private void Start()
    {
        foreach (Transform t in level.MovingObjectsContainer)
        {
            _queuedTweens[t] = new Queue<QueuedTweenInfo>();
        }
    }

    private void OnEnable()
    {
        EvtBus.Subscribe(this);
    }
    
    private void OnDisable()
    {
        EvtBus.Unsubscribe(this);
    }

    public void ApplyRotationOnBeat()
    {
        Vector3 axis = rotationAxis[_currentAxis];
        _currentAxis = _currentAxis < rotationAxis.Length - 1 ? _currentAxis + 1 : 0;
        float minX = -50f;
        
        foreach (Transform target in level.MovingObjectsContainer)
        {
            float delay = (target.localPosition.x - minX) / 100f;
            var queuedTweens = _queuedTweens[target];
            
            if (_currentlyTweening.TryGetValue(target, out var tween) && tween.isAlive)
            {
                queuedTweens.Enqueue(new QueuedTweenInfo
                {
                    axis = axis,
                    delay = delay - (tween.duration - tween.elapsedTime),
                    target = target
                });
                
                continue;
            }
            
            CreateRotationTween(target, axis, delay);
        }
    }

    private void CreateRotationTween(Transform target, Vector3 axis, float delay)
    {
        var tween = Tween.LocalRotation(target,target.localRotation * Quaternion.AngleAxis(180f, axis), .5f, Ease.Default, 1, CycleMode.Restart, delay);
        _currentlyTweening[target] = tween;

        tween.OnComplete(() => OnRotationTweenCompleted(target));
    }

    private void OnRotationTweenCompleted(Transform target)
    {
        CheckForQueuedTween(target);
    }

    private void CheckForQueuedTween(Transform target)
    {
        if (_queuedTweens[target].Count <= 0)
        {
            return;
        }

        var infos = _queuedTweens[target].Dequeue();
        CreateRotationTween(infos.target, infos.axis, infos.delay);
    }

    private void OnDestroy()
    {
        ClearAllTweens();
    }
    
    private void ClearAllTweens()
    {
        foreach (var tween in _currentlyTweening.Values.AsValueEnumerable().Where(t => t.isAlive))
        {
            tween.Stop();
        }

        _queuedTweens.Clear();
    }

    private struct QueuedTweenInfo
    {
        public Transform target;
        public float delay;
        public Vector3 axis;
    }

    public void OnEvent(in WillUnloadScene evt)
    {
        if (evt.SceneName != gameObject.scene.name)
        {
            return;
        }
        
        Debug.Log("Unloading scene, clearing all tweens.");
        ClearAllTweens();
    }
}

using System;
using PrimeTween;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    [SerializeField][Range(60, 144)] private int targetFps = 60;
    [SerializeField][Min(200)] private int tweenCapacity = 800;
    private void Start()
    {
        PrimeTweenConfig.SetTweensCapacity(tweenCapacity);
        Application.targetFrameRate = targetFps;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Game.Damage;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour, IEnemyBehaviour
{
    public EnemyFacade Facade { get; set; }

    [SerializeField] private Animator animator;
    [SerializeField] private List<AnimationInfo> animInfos;
    
    private Dictionary<AnimationType, AnimationInfo> _animHash;

    private enum AnimationType {Run, Attack, Idle, Death}
    
    public void PlayRunAnimation()
    {
        PlayAnimation(AnimationType.Run);
    }

    public void PlayAttackAnimation()
    {
        PlayAnimation(AnimationType.Attack);
    }

    private void PlayAnimation(AnimationType animType)
    {
        if (!_animHash.TryGetValue(animType, out AnimationInfo info))
        {
            Debug.LogError($"Animation of type {animType} not found in the animation dictionary !");
            return;
        }
        
        RefreshAnimationSpeed(animType);
        animator.SetTrigger(info.triggerName);
    }
    
    private void RefreshAnimationSpeed(AnimationType animType)
    {
        if (!_animHash.TryGetValue(animType, out AnimationInfo info))
        {
            Debug.LogError($"Animation of type {animType} not found in the animation dictionary !");
            return;
        }

        switch (animType)
        {
            case AnimationType.Run:
                break;
            case AnimationType.Attack:
                RefreshAttackAnimationSpeed(info);
                break;
            case AnimationType.Idle:
                break;
            case AnimationType.Death:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(animType), animType, null);
        }
    }

    private void RefreshAttackAnimationSpeed(AnimationInfo info)
    {
        float targetSpeed = info.BaseAnimationLength * Calculator.ComputeAttackSpeed(Facade.EnemyData.AttackSpeed, Facade.Stats.AttackSpeed.Value);
        int hash = info.SpeedMultiplierHash;
        if (hash == 0)
        {
            return;
        }
        
        animator.SetFloat(info.SpeedMultiplierHash, targetSpeed);
    }
    
    public void OnEnemyCreated()
    {
        _animHash = new Dictionary<AnimationType, AnimationInfo>();
        
        foreach (var info in animInfos)
        {
            info.BakeAnimationLength(animator);
            if (!_animHash.TryAdd(info.animationType, info))
            {
                Debug.LogError($"Key {info.animationType} is already present in the animation dictionary !");
            }
        }
    }

    public void OnEnemyRequested()
    {
    }

    public void OnEnemyReleased()
    {
    }
    
    [Serializable]
    private struct AnimationInfo
    {
        public float BaseAnimationLength => _baseAnimationLength;
        
        public string triggerName;
        public AnimationType animationType;
        public string speedMultiplierParam;
        public string clipName;
        
        [NonSerialized] private int _speedMulHash;
        [NonSerialized] private float _baseAnimationLength;
        public int SpeedMultiplierHash => _speedMulHash == 0 ? _speedMulHash = Animator.StringToHash(speedMultiplierParam) : _speedMulHash;
        
        public void BakeAnimationLength(Animator animator)
        {
            if (string.IsNullOrEmpty(clipName))
            {
                return;
            }

            var s = clipName;
            AnimationClip clip = animator.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name == s);
            
            if (!clip)
            {
                Debug.LogError($"Animation clip {clipName} not found in the animator controller for animation type {animationType}");
                return;
            }

            _baseAnimationLength = clip.length;
        }
    }
}
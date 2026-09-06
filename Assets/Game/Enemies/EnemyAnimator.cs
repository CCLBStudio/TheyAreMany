using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimator : MonoBehaviour, IEnemyBehaviour
{
    public EnemyFacade Facade { get; set; }

    [SerializeField] private Animator animator;
    [SerializeField] private List<AnimationInfo> animInfos;
    private Dictionary<AnimationType, string> _animHash;

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
        if (!_animHash.TryGetValue(animType, out string hash))
        {
            if (!TryCreateMissingAnim(animType, out string newHash))
            {
                return;
            }

            animator.SetTrigger(newHash);
            return;
        }
        
        animator.SetTrigger(hash);
    }

    private bool TryCreateMissingAnim(AnimationType type, out string result)
    {
        int i = animInfos.FindIndex(x => x.animationType == type);
        if (i < 0)
        {
            Debug.LogError($"Unable to create missing animation of type {type}");
            result = string.Empty;
            return false;
        }

        result = animInfos[i].triggerName;
        _animHash.Add(animInfos[i].animationType, result);
        return true;
    }
    
    public void OnEnemyCreated()
    {
        _animHash = new Dictionary<AnimationType, string>();
        
        foreach (var info in animInfos)
        {
            if (_animHash.ContainsKey(info.animationType))
            {
                Debug.LogError($"Key {info.animationType} is already present in the animation dictionary !");
                continue;
            }
            
            _animHash.Add(info.animationType, info.triggerName);
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
        public string triggerName;
        public AnimationType animationType;
    }
}
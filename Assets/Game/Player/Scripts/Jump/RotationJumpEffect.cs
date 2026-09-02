using PrimeTween;
using CCLBStudio.ScriptableValue;
using UnityEngine;

[CreateAssetMenu(menuName = "They Are Many/Player/Jump/Effects/Add Torque Jump Effect", fileName = "NewAddTorqueJumpEffect")]
public class RotationJumpEffect : JumpEffect
{
    [SerializeField] private float duration = 1f;
    [SerializeField][Range(0f, 1f)] private float minNormalizedValueToRotate = .2f;
    [SerializeField] private Ease rotationEase = Ease.OutCubic;
    [SerializeField] private int maxTurns = 3;
    [SerializeField] private FloatValue normalizedJumpStrength;

    private Tween _rotationTween;

    private int GetTurnCount()
    {
        if(normalizedJumpStrength.Value < minNormalizedValueToRotate)
        {
            return 0;
        }

        float normalizedN = (normalizedJumpStrength.Value - minNormalizedValueToRotate) / (1f - minNormalizedValueToRotate);
        return Mathf.Clamp((int)(normalizedN * (maxTurns - 1)) + 1, 1, maxTurns);
    }
    
    public override void ChargingJump(PlayerJumper jumper)
    {
    }

    public override void Jump(PlayerJumper jumper)
    {
        int turns = GetTurnCount();
        Vector3 rotAxis = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        _rotationTween = Tween.Custom(0f, 360f * turns, duration, angle => jumper.rotationTransform.rotation = Quaternion.AngleAxis(angle, rotAxis), rotationEase);
    }

    public override void ApexReached(PlayerJumper jumper)
    {
    }

    public override void Landed(PlayerJumper jumper)
    {
    }

    public override void OnFixedUpdate(PlayerJumper jumper)
    {
    }

    public override void ChargingPropulsion(PlayerJumper jumper)
    {
    }

    public override void Propulse(PlayerJumper jumper)
    {
        if (_rotationTween.isAlive || !jumper.InRangePropulsor)
        {
            return;
        }

        Jump(jumper);
    }
}

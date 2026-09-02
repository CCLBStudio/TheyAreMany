using PrimeTween;
using CCLBStudio.ScriptableValue;
using UnityEngine;

[CreateAssetMenu(menuName = "They Are Many/Player/Jump/Effects/Anti Gravity Apex Effect", fileName = "NewAntiGravityApexJumpEffect")]
public class AntiGravityApexJumpEffect : JumpEffect
{
    [SerializeField][Min(0f)] private float antiGravityTime = 0.06f;
    [SerializeField][Min(0f)] private float smoothingTime = .25f;
    [SerializeField] private bool performAntiGravityOnPropulsion = true;
    [SerializeField] private FloatValue normalizedJumpStrength;

    private bool _canPerformEffect;
    private float _smoothedNormalizedForce;
    private bool _reachedApex;
    private float _timer;
    private Vector2 _antiGravityForce;

    private void AllowEffect()
    {
        _canPerformEffect = true;
        _reachedApex = false;
        _timer = antiGravityTime * normalizedJumpStrength.Value + smoothingTime;
    }

    private void StopEffect()
    {
        _canPerformEffect = false;
        _reachedApex = true;
        _timer = 0f;
    }
    
    public override void ChargingJump(PlayerJumper jumper)
    {
        
    }

    public override void Jump(PlayerJumper jumper)
    {
        AllowEffect();
    }

    public override void ApexReached(PlayerJumper jumper)
    {
        _reachedApex = true;
        _smoothedNormalizedForce = 1f;
        _antiGravityForce = -Physics2D.gravity * (jumper.movementRb.gravityScale * jumper.movementRb.mass);
        Tween.Custom(1f, 0f, smoothingTime, f => _smoothedNormalizedForce = f, Ease.Linear, 1, CycleMode.Restart, antiGravityTime * normalizedJumpStrength.Value);
    }

    public override void Landed(PlayerJumper jumper)
    {
        StopEffect();
    }

    public override void OnFixedUpdate(PlayerJumper jumper)
    {
        if (!_canPerformEffect)
        {
            return;
        }

        if (_reachedApex && _timer > 0f)
        {
            _timer -= Time.fixedDeltaTime;
            jumper.movementRb.AddForce(_antiGravityForce * _smoothedNormalizedForce);
        }
    }

    public override void ChargingPropulsion(PlayerJumper jumper)
    {
        if (performAntiGravityOnPropulsion)
        {
            AllowEffect();
        }
        else
        {
            StopEffect();
        }
    }

    public override void Propulse(PlayerJumper jumper)
    {
    }
}

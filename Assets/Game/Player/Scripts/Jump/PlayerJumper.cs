using CCLBStudio.ScriptableValue;
using System.Collections.Generic;
using CCLBStudio.GlobalUpdater;
using UnityEngine;

[RequireComponent(typeof(PlayerGroundChecker))]
public class PlayerJumper : MonoBehaviour, IPlayerBehaviour, IFixedUpdate
{
    public bool IsJumping => _isJumping;
    public bool ReachedApex => _reachedApex;
    public bool Grounded => _groundChecker.Grounded;
    public bool IsChargingJump => _isChargingJump;
    public PlayerFacade Facade { get; set; }
    public Propulsor InRangePropulsor { get; private set; }

    public Rigidbody2D movementRb;
    public Transform scaleTransform;
    public Transform rotationTransform;

    [SerializeField] private InputReader inputReader;
    [SerializeField] private FloatValue normalizedJumpStrength;
    [SerializeField] private FloatValue pressTimeForMaxJump;
    [SerializeField] private Vector2Value propulsionDirection;
    [SerializeField] private float inputBufferTime = .15f;
    [SerializeField] private List<JumpEffect> jumpEffects;

    private bool _isJumping;
    private bool _isChargingJump;
    private bool _reachedApex;

    private float _pressingTime;
    private float _beginChargeTime;
    private bool _hasPressedJumpInput;

    private Vector3 _previousPosition;
    private bool _hasPressedPropulseInput;
    private PlayerGroundChecker _groundChecker;
    private Dictionary<Propulsor, bool> _hasPropulsed = new();
    
    public void Initialize()
    {
        _previousPosition = movementRb.linearVelocity;
        _groundChecker = GetComponent<PlayerGroundChecker>();
        _isChargingJump = false;
        _isJumping = false;
        _reachedApex = false;

        inputReader.JumpBeginEvent += OnJumpInputPressed;
        inputReader.JumpReleaseEvent += OnJumpInputReleased;
        inputReader.PropulsionBeginEvent += OnPropulsionInputPressed;
        inputReader.PropulsionReleaseEvent += OnPropulsionInputReleased;
        inputReader.MoveEvent += OrientPropulsion;
    }
    
    public void FixedTick()
    {
        TriggerFixedUpdateCallback();

        if(RequireApexReachedCallback())
        {
            _reachedApex = true;
            TriggerApexReachCallback();
        }
        
        _previousPosition = movementRb.position;
    }
    
    #region Jump Methods

    public void OnGrounded()
    {
        _isJumping = false;

        foreach (var effect in jumpEffects)
        {
            effect.Landed(this);
        }

        if (_hasPressedJumpInput && Time.time - _pressingTime <= inputBufferTime)
        {
            BeginJumpCharge();
        }
    }

    private void OnJumpInputPressed()
    {
        _pressingTime = Time.time;
        _hasPressedJumpInput = true;
        BeginJumpCharge();
    }

    private void OnJumpInputReleased()
    {
        _hasPressedJumpInput = false;

        if (!_isChargingJump)
        {
            return;
        }
            
        _isChargingJump = false;
        _isJumping = true;
        _reachedApex = false;
        normalizedJumpStrength.Value = Mathf.Clamp01((Time.time - _beginChargeTime) / pressTimeForMaxJump.Value);

        foreach (var effect in jumpEffects)
        {
            effect.Jump(this);
        }
    }

    private void BeginJumpCharge()
    {
        if (_isJumping)
        {
            return;
        }
            
        _beginChargeTime = Time.time;
        _isChargingJump = true;
        _reachedApex = false;
        normalizedJumpStrength.Value = 0f;
        _previousPosition = Vector3.zero;

        foreach (var effect in jumpEffects)
        {
            effect.ChargingJump(this);
        }
    }

    private bool RequireApexReachedCallback()
    {
        if (!IsJumping || _reachedApex)
        {
            return false;
        }
        
        Vector2 dir = movementRb.position - (Vector2)_previousPosition;
        return dir.y < 0f && !_reachedApex;
    }

    private void TriggerApexReachCallback()
    {
        foreach (var effect in jumpEffects)
        {
            effect.ApexReached(this);
        }
    }

    private void TriggerFixedUpdateCallback()
    {
        foreach (var effect in jumpEffects)
        {
            effect.OnFixedUpdate(this);
        }
    }

    #endregion

    #region Propulsion Methods
    
    public void OnEnterPropulsor(Propulsor propulsor)
    {
        InRangePropulsor = propulsor;
        _hasPropulsed.TryAdd(propulsor, false);
    }

    public void OnExitPropulsor(Propulsor propulsor)
    {
        InRangePropulsor = null;
        _hasPropulsed.Remove(propulsor);
    }
    
    private void OnPropulsionInputPressed()
    {
        if (!InRangePropulsor)
        {
            return;
        }

        if (!_hasPropulsed.ContainsKey(InRangePropulsor) || _hasPropulsed[InRangePropulsor] == true)
        {
            return;
        }

        _previousPosition = Vector3.zero;
        _hasPressedPropulseInput = true;
        
        foreach (var effect in jumpEffects)
        {
            effect.ChargingPropulsion(this);
        }
    }

    private void OnPropulsionInputReleased()
    {
        if (!_hasPressedPropulseInput || InRangePropulsor == null)
        {
            return;
        }

        if (!_hasPropulsed.ContainsKey(InRangePropulsor) || _hasPropulsed[InRangePropulsor] == true)
        {
            return;
        }

        _hasPropulsed[InRangePropulsor] = true;
        _hasPressedPropulseInput = false;
        _reachedApex = false;
        _previousPosition = Vector3.zero;
        
        foreach (var effect in jumpEffects)
        {
            effect.Propulse(this);
        }
    }

    private void OrientPropulsion(Vector2 direction)
    {
        if (direction == Vector2.zero)
        {
            propulsionDirection.Value = Vector2.up;
            return;
        }
        
        propulsionDirection.Value = direction.normalized;
    }

    #endregion
}

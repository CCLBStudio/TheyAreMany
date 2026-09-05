using CCLBStudio.GlobalUpdater;
using UnityEngine;

public class PlayerMover : MonoBehaviour, IPlayerBehaviour, IFixedUpdate
{
    public bool IsMoving { get; private set; }
    public PlayerFacade Facade { get; set; }

    [SerializeField] private InputReader inputReader;
    [SerializeField] private float groundedMoveSpeed = 250f;
    [SerializeField] private float inAirHorizontalVelocityTarget = 10f;
    [SerializeField] private PlayerJumper jumper;
    [SerializeField] private Rigidbody2D rb;

    private Vector2 _currentDirection;
    private bool _propulsorInRange;
    private bool _chargingPropulsion;

    public void Initialize()
    {
        inputReader.MoveEvent += OnMoveInput;
        inputReader.PropulsionBeginEvent += OnPropulsionInputPressed;
        inputReader.PropulsionReleaseEvent += OnPropulsionInputReleased;
    }

    public void FixedTick()
    {
        if (!IsMoving || jumper.IsChargingJump || _chargingPropulsion)
        {
            return;
        }
        
        Move();
    }

    private void OnMoveInput(Vector2 dir)
    {
        IsMoving = dir != Vector2.zero;
        _currentDirection = dir;
    }

    private void Move()
    {
        rb.AddForceX(_currentDirection.x * groundedMoveSpeed);
        if (!jumper.Grounded)
        {
            float sign = Mathf.Sign(rb.linearVelocityX);

            float absVel = Mathf.Clamp((Mathf.Abs(rb.linearVelocityX) - inAirHorizontalVelocityTarget), 0f, 1f);
            rb.AddForceX(-sign * absVel * groundedMoveSpeed);
        }
    }
    
    #region Propulsion Methods
    
    public void OnEnterPropulsor(Propulsor propulsor)
    {
        _propulsorInRange = true;
    }

    public void OnExitPropulsor(Propulsor propulsor)
    {
        _propulsorInRange = false;
    }

    private void OnPropulsionInputPressed()
    {
        _chargingPropulsion = _propulsorInRange;
    }

    private void OnPropulsionInputReleased()
    {
        _chargingPropulsion = false;
    }

    #endregion
}

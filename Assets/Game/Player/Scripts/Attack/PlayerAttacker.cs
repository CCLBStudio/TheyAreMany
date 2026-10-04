using CCLBStudio.GlobalUpdater;
using Game.ModularWeapon;
using Game.Player;
using Game.Weapon;
using UnityEngine;

public class PlayerAttacker : MonoBehaviour, IPlayerBehaviour, IUpdate, IWeaponOwner, IKnockbackTarget
{
    public PlayerFacade Facade { get; set; }
    public Transform WeaponContainer => weaponHolder;
    
    [SerializeField] private InputReader inputReader;
    [SerializeField] private ScriptableWeapon startWeapon;
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Rigidbody2D playerRb;
    
    private bool _isShooting;
    private bool _startShooting;
    private float _shootingTimer;
    private Vector2 _shootingDirection;
    private RuntimeWeapon _currentWeapon;
    private PlayerJumper _jumper;
    
    public void Initialize()
    {
        inputReader.AimEvent += OnAim;
        _currentWeapon = startWeapon.Equip(this);
        _jumper = Facade.GetBehaviour<PlayerJumper>();
    }

    public void Tick()
    {
        weaponPivot.rotation = Quaternion.FromToRotation(Vector3.right, _shootingDirection);
        _currentWeapon.ShootingDirection = _shootingDirection;
    }
    
    private void OnAim(Vector2 direction)
    {
        bool shooting = direction != Vector2.zero;

        switch (_isShooting)
        {
            case false when shooting:
                _currentWeapon.StartShooting();
                break;
            
            case true when !shooting:
                _currentWeapon.StopShooting();
                break;
        }

        _isShooting = shooting;
        _shootingDirection = direction.normalized;
    }

    private bool IsGrounded()
    {
        return !_jumper || _jumper.Grounded; // true if no jumper component
    }

    public void ApplyKnockback(Vector3 direction, Vector3 inAirModifier)
    {
        direction.x *= IsGrounded() ? 1f : inAirModifier.x;
        direction.y *= IsGrounded() ? 1f : inAirModifier.y;
        
        playerRb.AddForce(direction, ForceMode2D.Impulse);
    }
}

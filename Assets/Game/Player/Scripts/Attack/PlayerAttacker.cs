using CCLBStudio.GlobalUpdater;
using UnityEngine;

public class PlayerAttacker : MonoBehaviour, IPlayerBehaviour, IUpdate
{
    public Transform WeaponHolder => weaponHolder;
    public Rigidbody2D PlayerRb => playerRb;
    public PlayerJumper Jumper { get; private set; }
    public PlayerFacade Facade { get; set; }

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
    
    public void Initialize()
    {
        inputReader.AimEvent += OnAim;
        _currentWeapon = startWeapon.Equip(this);
        Jumper = GetComponent<PlayerJumper>();
    }

    public void Tick()
    {
        weaponPivot.rotation = Quaternion.FromToRotation(Vector3.right, _shootingDirection);
        
        if(_shootingTimer > 0f)
        {
            _shootingTimer -= Time.deltaTime;
            return;
        }

        if(!_isShooting)
        {
            return;
        }
        
        _currentWeapon.Shoot(_shootingDirection);
        _shootingTimer = 1f / _currentWeapon.AttackSpeed;
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
}

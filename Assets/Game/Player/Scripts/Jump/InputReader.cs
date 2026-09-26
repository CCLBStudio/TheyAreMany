using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

[CreateAssetMenu(fileName = "NewInputReader", menuName = "CCLB Studio/Inputs/InputReader")]
public partial class InputReader : ScriptableObject, PlayerControls.IPlayerActions
{
    [SerializeField] private bool autoInit = true;
    [SerializeField] private PlayerId playerId = PlayerId.Player1;
    
    public event UnityAction<Vector2> MoveEvent;
    public event UnityAction<Vector2> AimEvent;
    public event UnityAction JumpBeginEvent;
    public event UnityAction JumpReleaseEvent;
    public event UnityAction PropulsionBeginEvent;
    public event UnityAction PropulsionReleaseEvent;
    public event UnityAction PrimaryAbilityPressEvent;
    public event UnityAction PrimaryAbilityReleaseEvent;

    [NonSerialized] private PlayerControls _playerInputs;
    [NonSerialized] private InputDevice _assignedDevice;

    private static bool _globalInitPerformed = false;
    private static Dictionary<PlayerId, InputDevice> _playerDevices;
    
    private enum PlayerId {Player1 = 0, Player2 = 1, Player3 = 2, Player4 = 3}

    private void OnEnable()
    {
        Clear();
        
        if (autoInit)
        {
            Init();
        }
    }

    private void OnDisable()
    {
        Clear();
    }

    public void Init()
    {
        if (_playerInputs != null)
        {
            return;
        }
        
        GlobalInit();

        _assignedDevice = _playerDevices[playerId];
        _playerInputs = new PlayerControls();
        _playerInputs.Player.SetCallbacks(this);
        _playerInputs.devices = new ReadOnlyArray<InputDevice>();
        
        InputSystem.onDeviceChange += OnDeviceChange;
        CheckForPlayerDevice();

        _playerInputs.Player.Enable();
        _playerInputs.UI.Disable();
    }

    private static void GlobalInit()
    {
        if (_globalInitPerformed)
        {
            return;
        }

        _globalInitPerformed = true;
        _playerDevices = new Dictionary<PlayerId, InputDevice>(4);

        foreach (PlayerId id in Enum.GetValues(typeof(PlayerId)))
        {
            _playerDevices[id] = null;
        }

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            if (i >= 4)
            {
                break;
            }

            PlayerId id = (PlayerId)i;
            _playerDevices[id] = Gamepad.all[i];
        }
    }

    public void Clear()
    {
        if (_playerInputs == null)
        {
            return;
        }
        
        DisableAll();
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (device is not Gamepad gamepad)
        {
            return;
        }

        if (_assignedDevice != null && _assignedDevice != device)
        {
            return;
        }
        
        Debug.Log($"Gamepad named {gamepad.displayName} with id {gamepad.GetHashCode()} changed state to : {change}.");
        
        switch (change)
        {
            case InputDeviceChange.Added:
                if (IsNewDevice(gamepad) && _assignedDevice == null)
                {
                    _playerDevices[playerId] = gamepad;
                    SetDevice(gamepad);
                }
                break;

            case InputDeviceChange.Removed:
                break;

            case InputDeviceChange.Disconnected:
                break;

            case InputDeviceChange.Reconnected:
                if (_assignedDevice == gamepad)
                {
                    SetDevice(gamepad);
                }
                break;
            
            case InputDeviceChange.Enabled:
                break;
            
            case InputDeviceChange.Disabled:
                break;
            
            case InputDeviceChange.UsageChanged:
                break;
            
            case InputDeviceChange.ConfigurationChanged:
                break;
            
            case InputDeviceChange.SoftReset:
                break;
            
            case InputDeviceChange.HardReset:
                break;
            
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, null);
        }
    }

    private bool IsNewDevice(InputDevice device)
    {
        return _playerDevices.Values.All(playerDevice => device != playerDevice);
    }

    private void CheckForPlayerDevice()
    {
        if (_playerDevices[playerId] == null)
        {
            Debug.LogWarning($"There is no gamepad available for {playerId}");
            return;
        }
        
        SetDevice(_playerDevices[playerId]);
    }
    
    public void SetDevice(InputDevice device)
    {
        if (device == null)
        {
            return;
        }
        
        _playerInputs.devices = new[] {device};
        _assignedDevice = device;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            MoveEvent?.Invoke(context.ReadValue<Vector2>());
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            MoveEvent?.Invoke(Vector2.zero);
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            AimEvent?.Invoke(context.ReadValue<Vector2>());
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            AimEvent?.Invoke(Vector2.zero);
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            JumpBeginEvent?.Invoke();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            JumpReleaseEvent?.Invoke();
        }
    }

    public void OnPropulse(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            PropulsionBeginEvent?.Invoke();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            PropulsionReleaseEvent?.Invoke();
        }
    }

    public void OnPrimaryAbility(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            PrimaryAbilityPressEvent?.Invoke();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            PrimaryAbilityReleaseEvent?.Invoke();
        }
    }

    public void EnablePlayerInputs()
    {
        _playerInputs.Player.Enable();
    }
    
    public void DisablePlayerInputs()
    {
        _playerInputs.Player.Disable();
    }
    
    public void EnableUiInputs()
    {
        _playerInputs.UI.Enable();
    }

    public void DisableUiInputs()
    {
        _playerInputs.UI.Disable();
    }

    public void DisableAll()
    {
        DisablePlayerInputs();
        DisableUiInputs();
    }
}

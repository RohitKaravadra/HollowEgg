using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    [SerializeField] MobileInputs _MobileInputs;

    PlayerInputActions _InputActions;
    public PlayerInputActions.PlayerActions Player => _InputActions.Player;
    public PlayerInputActions.UIActions UI => _InputActions.UI;

    public static bool HasInstance => Instance != null;

    PlayerInput _PlayerInput;

#if UNITY_ANDROID || UNITY_IOS
    private bool _IsMobile = true;
#else
    private bool _IsMobile = false;
#endif

    // singleton
    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        _InputActions = new PlayerInputActions();
        _PlayerInput = GetComponent<PlayerInput>();
        _PlayerInput.actions = _InputActions.asset;
        SetMobileInputs();
    }

    private void Start()
    {
        UI.Enable();

        if (!_IsMobile)
            SetPlayerInput(true);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (_InputActions != null)
        {
            _InputActions.Disable();
            _InputActions.Dispose();
            _InputActions = null;
        }
    }

    public void SetMobileInputs()
    {
        _IsMobile = Application.isMobilePlatform;
        if (_MobileInputs != null)
        {
            _MobileInputs.Enabled = _IsMobile;
            if (_IsMobile)
            {
                Cursor.lockState = CursorLockMode.None;
                Player.Disable();
            }
        }

        _PlayerInput.neverAutoSwitchControlSchemes = _IsMobile;
    }

    public void SetPlayerInput(bool val = true)
    {
        if (_IsMobile) return;
        if (val) Player.Enable();
        else Player.Disable();
        Cursor.lockState = val ? CursorLockMode.Locked : CursorLockMode.None;
    }

    public void BindPlayerMove(Action<Vector2> action, bool val = false)
    {
        if (_IsMobile)
        {
            if (_MobileInputs != null) _MobileInputs.BindPlayerMove(action, val);
            return;
        }
        else
        {
            if (val)
            {
                Player.Move.performed += ctx => action(ctx.ReadValue<Vector2>());
                Player.Move.canceled += ctx => action(Vector2.zero);
            }
            else
            {
                Player.Move.performed -= ctx => action(ctx.ReadValue<Vector2>());
                Player.Move.canceled -= ctx => action(Vector2.zero);
            }
        }
    }

    public void BindPlayerJump(Action<bool> action, bool val = false)
    {
        if (_IsMobile)
        {
            if (_MobileInputs != null) _MobileInputs.BindPlayerJump(action, val);
            return;
        }
        else
        {
            if (val)
            {
                Player.Jump.performed += ctx => action(true);
                Player.Jump.canceled += ctx => action(false);
            }
            else
            {
                Player.Jump.performed -= ctx => action(true);
                Player.Jump.canceled -= ctx => action(false);
            }
        }
    }

    public void BindPlayerAttack(Action action, bool val = false)
    {
        if (_IsMobile)
        {
            if (_MobileInputs != null) _MobileInputs.BindPlayerAttack(action, val);
            return;
        }
        else
        {
            if (val) Player.Attack.performed += ctx => action();
            else Player.Attack.performed -= ctx => action();
        }
    }

    public void BindPlayerDash(Action action, bool val = false)
    {
        if (_IsMobile)
        {
            if (_MobileInputs != null) _MobileInputs.BindPlayerDash(action, val);
            return;
        }
        else
        {
            if (val) Player.Sprint.performed += ctx => action();
            else Player.Sprint.performed -= ctx => action();
        }
    }
}

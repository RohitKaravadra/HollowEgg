using System;
using System.Data;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private bool _Paused;
    private PlayerInputActions _PlayerInputs;
    private PlayerInputActions.UIActions UI => _PlayerInputs.UI;

    public PlayerInputActions.PlayerActions Player => _PlayerInputs.Player;

    public static bool HasInstance => Instance != null;

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

        _PlayerInputs = new PlayerInputActions();
        _PlayerInputs.Enable();
    }

    private void Start()
    {
        _Paused = false;
        Cursor.lockState = _Paused ? CursorLockMode.None : CursorLockMode.Locked;

        UI.Enable();

        if (_Paused) Player.Disable();
        else Player.Enable();
    }
}

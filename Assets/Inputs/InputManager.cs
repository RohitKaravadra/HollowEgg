using System;
using System.Data;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private PlayerInputActions _PlayerInputs;
    public PlayerInputActions.PlayerActions Player => _PlayerInputs.Player;
    public PlayerInputActions.UIActions UI => _PlayerInputs.UI;

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

    public void SetPlayerInput(bool val = true)
    {
        if (val) Player.Enable();
        else Player.Disable();
        Cursor.lockState = val ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void Start() => SetPlayerInput(true);
}

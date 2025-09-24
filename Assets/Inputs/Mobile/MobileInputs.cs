using System;
using UnityEngine;

public class MobileInputs : MonoBehaviour
{
    [SerializeField] UIVirtualJoystick _MovementJoystick;
    [SerializeField] UIVirtualButton _MobileJumpButton;
    [SerializeField] UIVirtualButton _MobileAttackButton;
    [SerializeField] UIVirtualButton _MobileDashButton;

    public bool Enabled
    {
        get => gameObject.activeSelf;
        set => gameObject.SetActive(value);
    }

    public void BindPlayerMove(Action<Vector2> action, bool val = false)
    {
        if (_MovementJoystick == null) return;
        if (val) _MovementJoystick.joystickOutputEvent
                .AddListener(new UnityEngine.Events.UnityAction<Vector2>(action));
        else _MovementJoystick.joystickOutputEvent
                .RemoveListener(new UnityEngine.Events.UnityAction<Vector2>(action));
    }

    public void BindPlayerJump(Action<bool> action, bool val = false)
    {
        if (_MobileJumpButton == null) return;
        if (val) _MobileJumpButton.buttonStateOutputEvent
                .AddListener(new UnityEngine.Events.UnityAction<bool>(action));
        else _MobileJumpButton.buttonStateOutputEvent
                .RemoveListener(new UnityEngine.Events.UnityAction<bool>(action));
    }

    public void BindPlayerAttack(Action action, bool val = false)
    {
        if (_MobileAttackButton == null) return;
        if (val) _MobileAttackButton.buttonClickOutputEvent
                .AddListener(new UnityEngine.Events.UnityAction(action));
        else _MobileAttackButton.buttonClickOutputEvent
                .RemoveListener(new UnityEngine.Events.UnityAction(action));
    }

    public void BindPlayerDash(Action action, bool val = false)
    {
        if (_MobileDashButton == null) return;
        if (val) _MobileDashButton.buttonClickOutputEvent
                .AddListener(new UnityEngine.Events.UnityAction(action));
        else _MobileDashButton.buttonClickOutputEvent
                .RemoveListener(new UnityEngine.Events.UnityAction(action));
    }
}

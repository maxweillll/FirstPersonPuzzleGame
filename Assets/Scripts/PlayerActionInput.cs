using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 把 Input System 的消息转成 C# 事件，方便其他脚本订阅
// 挂在 Player 身上，Input System 会自动调用这里的 OnChangeRoom / OnGetPickup
public class PlayerActionInput : MonoBehaviour
{
    public event Action OnChangeRoomPressed;
    public event Action OnGetPickupPressed;
    public event Action OnOpenMenuPressed;

    // 由 Input System 自动调用（通过 StarterAssets.inputactions 中的 Invoke Unity Events 绑定）
    public void OnChangeRoom(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnChangeRoomPressed?.Invoke();
    }

    public void OnGetPickup(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnGetPickupPressed?.Invoke();
    }
    public void OnOpenMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            OnOpenMenuPressed?.Invoke();
        }
    }
}

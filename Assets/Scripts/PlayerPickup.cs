using UnityEngine;
using UnityEngine.InputSystem;

// 挂在玩家身上，处理"看物品 → 高亮 → 按 E 拾取 → 按 E 放下"的交互
public class PlayerPickup : MonoBehaviour
{
    // ==========  Inspector 字段  ==========

    [Header("检测设置")]
    [Tooltip("只有这个 Layer 上的物体才能被拾取")]
    public LayerMask pickupLayer;
    [Tooltip("射线检测距离")]
    public float pickupDistance = 2f;

    [Header("手持位置")]
    [Tooltip("物品拾取后放在哪个 Transform 下（比如玩家手上的空物体）")]
    public Transform holdPoint;

    [Header("相机")]
    [Tooltip("从哪个相机发射射线，不填自动用 Camera.main")]
    public Camera playerCamera;

    // ==========  私有变量  ==========

    private PickupObject currentTarget;  // 当前正在看的可拾取物品
    private PickupObject heldObject;     // 当前手持的物品

    /// <summary>当前手持的物品（只读），供外部（如 PlayerPersistent）传送后瞬移</summary>
    public PickupObject HeldObject => heldObject;

    // ==========  Unity 生命周期  ==========

    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;
    }

    void Update()
    {
        // 手里有东西时不做瞄准
        if (heldObject != null) return;

        DetectPickupTarget();
    }

    // ==========  输入回调（由 Input System 调用）  ==========

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (heldObject != null)
            ReleaseObject();
        else if (currentTarget != null)
            PickUpObject();
    }

    // ==========  拾取 / 放下  ==========

    void PickUpObject()
    {
        heldObject = currentTarget;
        currentTarget.OnDeselect();
        heldObject.PickUp(holdPoint);
        heldObject.OnForceDrop += OnObjectForceDropped;
        currentTarget = null;
    }

    void ReleaseObject()
    {
        if (heldObject == null) return;

        heldObject.OnForceDrop -= OnObjectForceDropped;
        heldObject.Drop();
        heldObject = null;
    }

    // 物品被卡住自动脱手时 PickupObject 会通知这里
    void OnObjectForceDropped()
    {
        if (heldObject != null)
        {
            heldObject.OnForceDrop -= OnObjectForceDropped;
            heldObject = null;
        }
    }

    // ==========  瞄准检测  ==========

    void DetectPickupTarget()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupDistance, pickupLayer))
        {
            PickupObject pickup = hit.collider.GetComponent<PickupObject>();

            // 看的是同一个物体，不做任何事
            if (pickup == currentTarget) return;

            // 切换目标：旧的取消高亮，新的高亮
            if (currentTarget != null)
                currentTarget.OnDeselect();

            currentTarget = pickup;

            if (currentTarget != null)
                currentTarget.OnSelect();
        }
        else
        {
            // 没看到任何可拾取物
            if (currentTarget != null)
            {
                currentTarget.OnDeselect();
                currentTarget = null;
            }
        }
    }

    // ==========  编辑器辅助  ==========

    void OnDrawGizmosSelected()
    {
        Vector3 origin = playerCamera != null
            ? playerCamera.transform.position
            : transform.position;
        Vector3 direction = playerCamera != null
            ? playerCamera.transform.forward
            : transform.forward;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, direction * pickupDistance);
        Gizmos.color = Color.red * 0.5f;
        Gizmos.DrawWireSphere(origin + direction * pickupDistance, 0.1f);
    }
}

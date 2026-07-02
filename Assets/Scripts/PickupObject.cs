using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// 挂在可被拾取的物品上
// 负责：物品高亮 / 跟随手持点 / 碰墙停住 / 卡太远脱手 / 跨场景持久化
public class PickupObject : MonoBehaviour
{
    // ==========  Inspector 字段  ==========

    [Header("基本设置")]
    [Tooltip("是否允许被拾取")]
    public bool canPick = true;
    [Tooltip("被瞄准时的高亮颜色")]
    public Color highlightColor = Color.green;

    [Header("碰撞检测")]
    [Tooltip("物品持握时能和哪些层碰撞（建议在 Inspector 里去掉 Player 层）")]
    public LayerMask collisionMask = ~0;

    [Header("脱手")]
    [Tooltip("物品被卡住后，实际位置和手持点距离超过这个值就自动脱手")]
    public float dropDistanceThreshold = 1.5f;

    [Header("跨场景去重")]
    [Tooltip("物品唯一标识，同一个物品在不同场景中的副本应设置相同的 ID")]
    public string itemID;

    // ==========  静态：跨场景状态追踪  ==========

    // 已捡过的物品 ID（永久记录，阻止场景重载时刷出副本）
    private static HashSet<string> pickedUpItemIDs = new HashSet<string>();

    // 已丢下的物品：itemID → 物品实例引用，用于场景加载时激活/隐藏
    private static Dictionary<string, DroppedItem> droppedItems = new Dictionary<string, DroppedItem>();

    private struct DroppedItem
    {
        public PickupObject instance;
        public string sceneName;
        public Vector3 position;
        public Quaternion rotation;
    }

    // ==========  事件  ==========

    // 物品被强制脱手时触发，PlayerPickup 订阅它来清理自己手里的引用
    public event Action OnForceDrop;

    // ==========  组件引用  ==========

    private MeshRenderer meshRenderer;
    private Rigidbody rigidBody;
    private Collider myCollider;
    private Collider playerCollider;

    // ==========  状态  ==========

    private Transform holdPoint;         // 手持点 Transform（由 PlayerPickup 传入）
    private bool isHeld = false;
    private bool playerColliderCached = false;
    private Color originalColor;         // 物品本来的颜色
    private PhysicMaterial zeroFrictionMaterial;  // 零摩擦物理材质（拾取时应用）

    // ==========  Unity 生命周期  ==========

    void Awake()
    {
        // 跨场景去重：如果这个 itemID 已经被捡过，
        // 说明当前是场景重载后新生成的副本，直接销毁
        if (!string.IsNullOrEmpty(itemID) && pickedUpItemIDs.Contains(itemID))
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        originalColor = meshRenderer.material.color;

        rigidBody = GetComponent<Rigidbody>();
        myCollider = GetComponent<Collider>();

        if (rigidBody == null || myCollider == null)
        {
            Debug.LogError($"【PickupObject】{gameObject.name} 缺少 Rigidbody 或 Collider，请检查");
            enabled = false;
            return;
        }

        // 创建零摩擦材质（拾取时切换到 collider）
        zeroFrictionMaterial = new PhysicMaterial();
        zeroFrictionMaterial.staticFriction = 0f;
        zeroFrictionMaterial.dynamicFriction = 0f;
        zeroFrictionMaterial.bounciness = 0f;
        zeroFrictionMaterial.frictionCombine = PhysicMaterialCombine.Minimum;

        // 订阅场景加载事件，用于激活/隐藏已丢下的物品
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void LateUpdate()
    {
        if (!isHeld || holdPoint == null) return;

        // 把物品移到手持点，途中碰墙就停
        MoveToHoldPoint(holdPoint.position, holdPoint.rotation);

        // 距离太远 → 脱手
        float distance = Vector3.Distance(transform.position, holdPoint.position);
        if (distance > dropDistanceThreshold)
        {
            Debug.Log($"【PickupObject】{gameObject.name} 被卡住，距离 {distance:F2}，自动脱手");
            ForceDrop();
        }
    }

    // ==========  场景加载回调（静态，由任意实例处理所有丢下的物品）  ==========

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        foreach (var kv in droppedItems)
        {
            DroppedItem dropped = kv.Value;
            if (dropped.instance == null) continue;

            bool belongsHere = dropped.sceneName == sceneName;

            // 在本场景 → 激活并归位；不在本场景 → 隐藏
            dropped.instance.gameObject.SetActive(belongsHere);

            if (belongsHere)
            {
                dropped.instance.transform.position = dropped.position;
                dropped.instance.transform.rotation = dropped.rotation;
            }
        }
    }

    // ==========  公开方法：高亮  ==========

    public void OnSelect()
    {
        meshRenderer.material.color = canPick ? highlightColor : Color.white;
    }

    public void OnDeselect()
    {
        meshRenderer.material.color = originalColor;
    }

    // ==========  公开方法：拾取 / 放下  ==========

    public void PickUp(Transform holdPointTransform)
    {
        if (!canPick || isHeld) return;

        isHeld = true;
        holdPoint = holdPointTransform;
        rigidBody.isKinematic = true;  // 手持时不参与重力

        // 标记为跨场景不销毁，让物品能跟随玩家到另一个场景
        DontDestroyOnLoad(gameObject);

        // 从丢下的物品字典中移除（如果之前被丢过）
        if (!string.IsNullOrEmpty(itemID))
        {
            pickedUpItemIDs.Add(itemID);
            droppedItems.Remove(itemID);
        }

        // 确保物品是激活的
        gameObject.SetActive(true);

        // 拾取时：摩擦力设为 0，防止放下时"焊"在接触面上
        myCollider.material = zeroFrictionMaterial;

        // 物品原本放在桌面/地面上，collider 底部和表面接触。
        // SphereCast 从重叠位置出发会漏检，导致物品穿模撞飞桌面。
        // 先往上抬一下，脱离接触面，后续 MoveToHoldPoint 才能正确检测碰撞。
        float lift = GetCheckRadius() + 0.05f;
        transform.position += Vector3.up * lift;

        CachePlayerCollider();

        // 手持时忽略和玩家的碰撞，避免把玩家推走
        if (playerCollider != null && myCollider != null)
            Physics.IgnoreCollision(myCollider, playerCollider, true);
    }

    public void Drop()
    {
        if (!isHeld) return;

        isHeld = false;
        holdPoint = null;

        // 切换到物理模式
        rigidBody.isKinematic = false;
        myCollider.material = default;  // 恢复默认摩擦力

        // 记录丢下的场景和位置，加入 droppedItems 字典
        string currentScene = SceneManager.GetActiveScene().name;
        if (!string.IsNullOrEmpty(itemID))
        {
            droppedItems[itemID] = new DroppedItem
            {
                instance = this,
                sceneName = currentScene,
                position = transform.position,
                rotation = transform.rotation
            };
        }

        // 移动到当前活跃场景，再重新 DontDestroyOnLoad
        // 这样物品显示在当前场景但跨场景不销毁
        SceneManager.MoveGameObjectToScene(gameObject, SceneManager.GetActiveScene());
        DontDestroyOnLoad(gameObject);

        // 物品已捡过，不从 pickedUpItemIDs 移除 ——
        // 确保任何场景重载时副本都被销毁

        // 重置速度 + 唤醒，避免 kinematic→non-kinematic 切换残留接触
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        rigidBody.WakeUp();

        // 还原颜色
        meshRenderer.material.color = originalColor;

        // 恢复和玩家的碰撞
        if (playerCollider != null && myCollider != null)
            Physics.IgnoreCollision(myCollider, playerCollider, false);
    }

    // 供外部调用：传送后瞬移物品到手持点，跳过碰撞检测
    public void SnapToHoldPoint()
    {
        if (!isHeld || holdPoint == null) return;
        transform.position = holdPoint.position;
        transform.rotation = holdPoint.rotation;
    }

    // ==========  内部：碰撞感知移动  ==========

    void MoveToHoldPoint(Vector3 targetPosition, Quaternion targetRotation)
    {
        transform.rotation = targetRotation;

        Vector3 from = transform.position;
        Vector3 direction = targetPosition - from;
        float distance = direction.magnitude;

        if (distance < 0.001f) return;
        direction /= distance;

        float radius = GetCheckRadius();

        // 只用一次沿着移动方向的球形射线检测，
        // 不再提前检查目标位置（那个会造成水平移动时被下方桌面误判阻挡）
        if (Physics.SphereCast(from, radius, direction, out RaycastHit hit,
            distance, collisionMask, QueryTriggerInteraction.Ignore))
        {
            // 跳过自己和玩家
            if (hit.collider != myCollider && hit.collider != playerCollider)
            {
                // 只阻挡"撞向表面"的移动，沿着表面滑动不阻挡
                float dot = Vector3.Dot(direction, hit.normal);
                if (dot < 0f)
                {
                    float safeDistance = Mathf.Max(0f, hit.distance - 0.02f);
                    transform.position = from + direction * safeDistance;
                    return;
                }
            }
        }

        // 没有阻挡 → 直接到目标位置
        transform.position = targetPosition;
    }

    // ==========  内部：辅助方法  ==========

    // 根据碰撞器类型计算检测球半径
    float GetCheckRadius()
    {
        Vector3 scale = transform.lossyScale;

        if (myCollider is SphereCollider sphere)
        {
            float maxScale = Mathf.Max(scale.x, scale.y, scale.z);
            return sphere.radius * maxScale * 0.95f;
        }
        if (myCollider is BoxCollider box)
        {
            Vector3 scaledSize = Vector3.Scale(box.size, scale);
            return scaledSize.magnitude * 0.45f;  // 内切球近似
        }
        if (myCollider is CapsuleCollider capsule)
        {
            float maxScale = Mathf.Max(scale.x, scale.y, scale.z);
            return Mathf.Max(capsule.radius, capsule.height * 0.5f) * maxScale * 0.9f;
        }

        return 0.3f;  // 兜底
    }

    // 查找玩家碰撞器（只在第一次拾取时查找，之后复用）
    void CachePlayerCollider()
    {
        if (playerColliderCached) return;
        playerColliderCached = true;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerCollider = player.GetComponent<Collider>();
            if (playerCollider == null)
                playerCollider = player.GetComponentInChildren<Collider>();
        }
    }

    // 被卡住 → 通知 PlayerPickup → 执行 Drop
    void ForceDrop()
    {
        OnForceDrop?.Invoke();
        Drop();
    }
}

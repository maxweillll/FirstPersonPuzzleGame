using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistent : MonoBehaviour
{
    private static PlayerPersistent instance;

    void Awake()
    {
        // 单例：如果已存在就删掉新的，保证全局只有一个玩家
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 切换场景后，把玩家放到新场景的出生点
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject spawnPoint = GameObject.FindWithTag("SpawnPoint");
        if (spawnPoint != null)
        {
            // 直接复位位置，不用改父节点DontDestroyOnLoad 的对象没有场景父节点
            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation;
        }

        // 玩家传送后，把手持物品也瞬移过来，防止距离过大触发 ForceDrop
        PlayerPickup pickup = GetComponent<PlayerPickup>();
        if (pickup != null && pickup.HeldObject != null)
        {
            pickup.HeldObject.SnapToHoldPoint();
        }
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

// 挂在第一个场景的某个 GameObject 上（比如空物体 "GameManager"）
// 负责：首次启动时创建玩家 / 场景切换时冻结/恢复玩家
public class CreatePlayer : MonoBehaviour
{
    // ==========  Inspector 字段  ==========

    [Header("玩家")]
    public GameObject playerPrefab;

    [Header("场景名")]
    [Tooltip("第一次启动时玩家出场的场景")]
    public string firstSceneName = "Scene1";
    [Tooltip("中转用的 Loading 场景名，进入此场景时会冻结玩家防止下落")]
    public string loadingSceneName = "LoadScene";

    // ==========  静态 / 单例  ==========

    private static CreatePlayer instance;
    private static bool playerAlreadyCreated = false;

    // ==========  私有变量  ==========

    private GameObject playerInstance;

    // ==========  Unity 生命周期  ==========

    void Awake()
    {
        // 单例：场景切换后旧的那个不销毁，新的销毁
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // 让异步加载更快
        Application.backgroundLoadingPriority = ThreadPriority.High;

        // 第一次启动时创建玩家（只创建一次）
        if (!playerAlreadyCreated)
        {
            CreateNewPlayer();
            playerAlreadyCreated = true;
        }

        // 监听场景加载，在 Loading 场景里冻结玩家
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ==========  玩家创建  ==========

    void CreateNewPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("【CreatePlayer】玩家预制体未赋值，请在 Inspector 里拖入 PlayerCapsule");
            return;
        }

        playerInstance = Instantiate(playerPrefab);
        DontDestroyOnLoad(playerInstance);

        // 放到场景里的出生点
        PlacePlayerAtSpawn();
    }

    void PlacePlayerAtSpawn()
    {
        GameObject spawnPoint = GameObject.FindWithTag("SpawnPoint");
        if (spawnPoint != null)
        {
            playerInstance.transform.position = spawnPoint.transform.position;
            playerInstance.transform.rotation = spawnPoint.transform.rotation;
        }
    }

    // ==========  场景切换处理  ==========

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 确保有玩家引用
        if (playerInstance == null)
            playerInstance = GameObject.FindGameObjectWithTag("Player");
        if (playerInstance == null) return;

        CharacterController controller = playerInstance.GetComponent<CharacterController>();
        if (controller == null) return;

        // Loading 场景里没有地板，禁用 CharacterController 防止玩家下落
        // 游戏场景里重新启用
        bool isInLoadingScene = scene.name == loadingSceneName;
        controller.enabled = !isInLoadingScene;
    }
}

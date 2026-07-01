using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// 挂在门 / 传送触发区域上
// 玩家走进触发区域 → 按 F → 异步切换到目标场景
public class ChangeRoom : MonoBehaviour
{
    // ==========  Inspector 字段  ==========

    [Header("提示 UI")]
    public GameObject pressFTip;

    [Header("场景配置")]
    [Tooltip("要进入的目标场景名")]
    public string targetSceneName;
    [Tooltip("中转的 Loading 场景名")]
    public string loadingSceneName = "LoadScene";

    // ==========  私有变量  ==========

    private PlayerActionInput playerInput;
    private bool canEnterRoom = false;
    private bool isSwitching = false;

    // ==========  Unity 生命周期  ==========

    void Start()
    {
        pressFTip.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        // 只有带 PlayerActionInput 的玩家才能触发
        playerInput = other.GetComponent<PlayerActionInput>();
        if (playerInput == null) return;

        pressFTip.SetActive(true);
        canEnterRoom = true;
        playerInput.OnChangeRoomPressed += OnPlayerPressF;
    }

    private void OnTriggerExit(Collider other)
    {
        pressFTip.SetActive(false);
        canEnterRoom = false;

        // 离开时取消订阅，防止重复触发和内存泄漏
        if (playerInput != null)
        {
            playerInput.OnChangeRoomPressed -= OnPlayerPressF;
            playerInput = null;
        }
    }

    // ==========  输入回调  ==========

    void OnPlayerPressF()
    {
        if (canEnterRoom && !isSwitching)
            StartSceneSwitch();
    }

    // ==========  场景切换  ==========

    void StartSceneSwitch()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("【ChangeRoom】目标场景名未设置，请在 Inspector 里填写");
            return;
        }

        isSwitching = true;

        // 把目标场景写入静态数据，供 LoadUI 读取
        SceneLoadData.TargetSceneName = targetSceneName;
        SceneLoadData.UseIndex = false;

        // 异步加载中转场景，避免卡顿
        StartCoroutine(LoadLoadingSceneAsync());
    }

    IEnumerator LoadLoadingSceneAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(loadingSceneName);

        if (operation != null)
        {
            // 加载阶段不自动激活，等场景准备好了再切
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
                yield return null;

            operation.allowSceneActivation = true;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// 挂在 Loading 场景的 Canvas 上
// 场景加载后自动读取 SceneLoadData，异步加载目标场景并显示进度条
public class LoadUI : MonoBehaviour
{
    // ==========  Inspector 字段  ==========

    [Header("UI 绑定")]
    public GameObject loadingScreen;
    public Slider progressSlider;
    public Text progressText;

    [Header("参数")]
    [Tooltip("加载太快时至少显示多久，避免 UI 闪一下")]
    public float minDisplayTime = 0.3f;
    [Tooltip("进度条跟随真实进度的速度，越大越快")]
    public float progressSmoothSpeed = 3f;

    // ==========  Unity 生命周期  ==========

    void Start()
    {
        // 提高异步加载优先级，让场景加载更快完成
        Application.backgroundLoadingPriority = ThreadPriority.High;

        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        StartCoroutine(LoadTargetScene());
    }

    // ==========  异步加载  ==========

    IEnumerator LoadTargetScene()
    {
        // 显示 loading 画面
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        // 等一帧确保 UI 渲染出来
        yield return null;

        // 根据 SceneLoadData 确定要加载哪个场景
        AsyncOperation operation;

        if (SceneLoadData.UseIndex && SceneLoadData.TargetSceneIndex >= 0)
            operation = SceneManager.LoadSceneAsync(SceneLoadData.TargetSceneIndex);
        else if (!string.IsNullOrEmpty(SceneLoadData.TargetSceneName))
            operation = SceneManager.LoadSceneAsync(SceneLoadData.TargetSceneName);
        else
        {
            Debug.LogError("【LoadUI】没有设置目标场景，请检查 ChangeRoom 的 targetSceneName");
            yield break;
        }

        if (operation == null)
        {
            Debug.LogError("【LoadUI】无法创建异步加载操作");
            yield break;
        }

        // 不让场景自动激活，等进度条跑完再手动激活
        operation.allowSceneActivation = false;

        float displayedProgress = 0f;
        float startTime = Time.time;

        // 循环更新进度条，直到场景加载到 90%
        while (operation.progress < 0.9f)
        {
            // 真实进度最大到 0.9，映射到 0~1
            float realProgress = operation.progress / 0.9f;

            // 平滑追赶真实进度
            displayedProgress = Mathf.Lerp(displayedProgress, realProgress,
                Time.deltaTime * progressSmoothSpeed);

            // 不让进度后退
            displayedProgress = Mathf.Max(displayedProgress, realProgress * 0.9f);

            UpdateProgressUI(displayedProgress);
            yield return null;
        }

        // 显示 100%
        UpdateProgressUI(1f);

        // 如果加载太快，多显示一会儿让玩家看到进度条
        float elapsed = Time.time - startTime;
        if (elapsed < minDisplayTime)
            yield return new WaitForSeconds(minDisplayTime - elapsed);

        UpdateProgressUI(1f);

        // 等一帧让 UI 刷新，再激活场景
        // 这样场景激活瞬间的卡顿会被 loading 画面遮住
        yield return null;

        operation.allowSceneActivation = true;

        // 激活后再等一帧，让新场景的物体完成初始化
        yield return null;
    }

    void UpdateProgressUI(float progress)
    {
        if (progressSlider != null)
            progressSlider.value = progress;
        if (progressText != null)
            progressText.text = Mathf.RoundToInt(progress * 100) + "%";
    }
}

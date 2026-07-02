using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ==============================================
/// 面板管理器（单例）
/// ==============================================
/// 作用：管理所有 UI 面板的打开和关闭。
///
/// 核心流程：
///   1. 外部调用 Open("面板名")
///   2. 查字典找到预制体路径
///   3. 用 Resources.Load 加载预制体
///   4. Instantiate 创建到 Canvas 下
///   5. 调用面板的 Open() 显示
///
/// 怎么用：
///   UIManager.Instance.Open("PackagePanel");   // 打开背包
///
/// 怎么添加新面板：
///   在 pathDict 里加一行：
///   { "你的面板名", "文件夹/预制体文件名" }
///   预制体放在 Assets/Resources/Prefab/Panel/ 下
/// </summary>
public class UIManager
{
    // ---------- 单例 ----------

    /// <summary>
    /// 单例实例。static 构造函数保证一运行就初始化。
    /// </summary>
    public static UIManager Instance;

    // ---------- 面板路径字典 ----------

    /// <summary>
    /// 面板名 → Resources 下的路径（相对于 Resources/Prefab/Panel/）
    ///
    /// 比如 "PackagePanel" → "Package/PackagePanel"
    /// 完整路径 = "Prefab/Panel/Package/PackagePanel"（Unity 自动拼上 Resources/ 和 .prefab）
    ///
    /// 要加新面板？在这里加一行就行：
    /// { "MainPanel", "Main/MainPanel" },
    /// </summary>
    private Dictionary<string, string> pathDict = new Dictionary<string, string>
    {
        { "PackagePanel", "Package/PackagePanel" },
    };

    // ---------- 已打开面板字典 ----------

    /// <summary>
    /// 面板名 → 面板实例。
    /// 用于判断面板是否已经打开（防止重复打开），以及关闭时移除。
    /// </summary>
    private Dictionary<string, BasePanel> panelDict = new Dictionary<string, BasePanel>();

    // ---------- 构造函数 ----------

    /// <summary>
    /// static 构造函数：在第一次访问 Instance 之前自动执行。
    /// 比在属性 getter 里判空更简洁。
    /// </summary>
    static UIManager()
    {
        Instance = new UIManager();
    }

    // ---------- 核心方法 ----------

    /// <summary>
    /// 打开一个面板。
    ///
    /// 参数：name - 面板名（要和 pathDict 里配的一致）
    /// 返回：打开的面板实例，失败返回 null
    ///
    /// 流程：
    ///   ① 检查是否已打开 → 已打开就返回已有的
    ///   ② 查路径字典 → 没配置就报错
    ///   ③ Resources.Load 加载预制体 → 没有文件就报错
    ///   ④ 找或创建 Canvas（所有 UI 的根）
    ///   ⑤ Instantiate 创建实例，挂到 Canvas 下
    ///   ⑥ 获取 BasePanel 组件，记入字典，调用 Open() 显示
    /// </summary>
    public BasePanel Open(string name)
    {
        // ① 已经打开了？直接返回
        if (panelDict.ContainsKey(name))
            return panelDict[name];

        // ② 查路径表
        if (!pathDict.ContainsKey(name))
        {
            Debug.LogError("未找到面板路径: " + name);
            return null;
        }

        // ③ 加载预制体
        string fullPath = "Prefab/Panel/" + pathDict[name];
        GameObject prefab = Resources.Load<GameObject>(fullPath);
        if (prefab == null)
        {
            // 预制体不存在，检查一下：
            //   - 文件在 Assets/Resources/Prefab/Panel/ 下吗？
            //   - 文件名拼对了吗？
            Debug.LogError("预制体不存在: Resources/" + fullPath + ".prefab");
            return null;
        }

        // ④ 找 Canvas（所有 UI 必须挂在 Canvas 下才能显示）
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            // 场景没有 Canvas？自动创建一个
            canvas = new GameObject("Canvas").AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;  // 屏幕空间覆盖模式（2D UI）
        }

        // ⑤ 克隆预制体，放到 Canvas 下
        GameObject obj = Object.Instantiate(prefab, canvas.transform);
        obj.name = name;  // 改名方便调试

        // ⑥ 获取面板组件，记录 + 显示
        BasePanel panel = obj.GetComponent<BasePanel>();
        panelDict.Add(name, panel);   // 记入"已打开"字典
        panel.Open();                 // 显示面板
        return panel;
    }

    /// <summary>
    /// 面板关闭时的回调（由 BasePanel.Close() 调用）。
    /// 从已打开字典里移除，这样下次 Open 才会重新创建。
    /// </summary>
    public void OnPanelClosed(string name)
    {
        panelDict.Remove(name);
    }
}

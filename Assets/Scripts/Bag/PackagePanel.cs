using UnityEngine;

/// <summary>
/// ==============================================
/// 背包面板
/// ==============================================
/// 打开后显示玩家背包里所有物品的图标。
///
/// 工作原理：
///   Start() 时调用 Refresh()
///     → 清空旧的格子
///     → 从 GameManager 获取背包数据
///     → 为每个物品 Instantiate 一个格子预制体
///     → 调用格子的 SetData() 填充图标
///
/// 你在 Inspector 里需要拖好两个引用：
///   cellPrefab    - 格子的预制体（PackgeUiItem.prefab）
///   contentParent - 滚动区域的内容节点（ScrollView 的 Content）
/// </summary>
public class PackagePanel : BasePanel      // 继承 BasePanel，所以能 Open / Close
{
    /// <summary>
    /// 物品格子的预制体。
    /// 在 Inspector 里把 PackgeUiItem.prefab 拖到这里。
    /// </summary>
    public GameObject cellPrefab;

    /// <summary>
    /// 物品格子的父容器。
    /// 通常是 ScrollView 下面的 Content 物体。
    /// 拖到这里就行。
    /// </summary>
    public Transform contentParent;

    // ---------- Unity 生命周期 ----------

    /// <summary>
    /// Start：面板激活后第一帧调用。
    /// 这里调用 Refresh() 显示背包内容。
    /// </summary>
    private void Start()
    {
        Refresh();
    }

    // ---------- 核心方法 ----------

    /// <summary>
    /// 刷新整个背包显示。
    ///
    /// 步骤：
    ///   1. 安全检查（没拖引用就不执行）
    ///   2. 删除 Content 下的所有旧格子（倒着删更安全）
    ///   3. 获取背包数据
    ///   4. 遍历每个物品 → 克隆格子 → 填入数据
    ///
    /// 什么时候调用？
    ///   - 面板第一次打开时（Start 自动调用）
    ///   - 捡到新物品后（外部手动调用 panel.Refresh()）
    /// </summary>
    public void Refresh()
    {
        // === 第1步：安全检查 ===
        // 如果没拖引用，后面的代码会报错，这里直接返回
        if (cellPrefab == null || contentParent == null)
            return;

        // === 第2步：清空旧格子 ===
        // 从后往前删，因为 Destroy 不会立即生效，倒着来避免索引错乱
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);

        // === 第3步：获取背包数据 ===
        var items = GameManager.Instance.GetBagItems();

        // === 第4步：创建格子 ===
        // Instantiate = 克隆预制体，就像复制粘贴
        // 第二个参数 contentParent = 克隆出来的物体挂到 Content 下
        foreach (PackageLocalItem item in items)
        {
            // 克隆一个格子
            GameObject cell = Instantiate(cellPrefab, contentParent);

            // 获取格子上的脚本，调用 SetData 填充数据（显示图标）
            cell.GetComponent<PackageCell>().SetData(item);
        }
    }
}

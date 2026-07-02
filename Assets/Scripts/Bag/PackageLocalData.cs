using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ==============================================
/// 背包数据存档 / 读档
/// ==============================================
/// 把玩家背包里的物品列表转成 JSON 字符串，
/// 存到 PlayerPrefs（Unity 的本地硬盘存储）。
///
/// 数据流向：
///   内存（items 列表） ←→ JSON 字符串 ←→ PlayerPrefs（硬盘）
///
/// 为什么需要这个？
///   Unity 运行时数据在内存里，关游戏就没了。
///   必须存到硬盘，下次打开才能恢复。
///   PlayerPrefs 是 Unity 最简单的本地存储方式，
///   适合存少量数据（背包、设置等）。
///
/// JSON 长什么样？
///   {"items":[{"uid":"abc123","id":1,"num":1},{"uid":"def456","id":3,"num":5}]}
/// </summary>
public class PackageLocalData
{
    // ---------- 单例 ----------

    /// <summary>
    /// 单例实例。直接 new，不需要判空。
    /// </summary>
    public static PackageLocalData Instance = new PackageLocalData();

    // ---------- 缓存 ----------

    /// <summary>
    /// 背包物品列表的内存缓存。
    /// 第一次 Load() 时从硬盘读取，之后直接复用。
    /// 缓存的好处：不用每次都用 PlayerPrefs，PlayerPrefs 读硬盘很慢。
    /// </summary>
    private List<PackageLocalItem> items;

    // ---------- 存取方法 ----------

    /// <summary>
    /// 读取背包数据。
    ///
    /// 逻辑：
    ///   1. 如果内存里已经有了（items != null），直接返回，不读硬盘
    ///   2. 如果内存没有，去 PlayerPrefs 里找
    ///      - 找到了 → 把 JSON 字符串还原成 List
    ///      - 没找到 → 返回空列表（第一次玩游戏的情况）
    /// </summary>
    public List<PackageLocalItem> Load()
    {
        // ① 有缓存，直接返回
        if (items != null)
            return items;

        // ② 从硬盘读取
        string key = "BagData";  // PlayerPrefs 的键名，可以随便取
        if (PlayerPrefs.HasKey(key))
        {
            // 拿到 JSON 字符串，比如：
            // {"items":[{"uid":"abc","id":1,"num":1}]}
            string json = PlayerPrefs.GetString(key);

            // JsonUtility.FromJson：把 JSON 字符串还原成 C# 对象
            ListWrapper wrapper = JsonUtility.FromJson<ListWrapper>(json);
            items = wrapper.items;  // 提取出物品列表
        }
        else
        {
            // 没存过数据（第一次玩），给个空列表
            items = new List<PackageLocalItem>();
        }

        return items;
    }

    /// <summary>
    /// 保存背包数据到硬盘。
    ///
    /// 流程：
    ///   1. 把 List 包一层（JsonUtility 不能直接序列化裸 List）
    ///   2. 用 JsonUtility.ToJson 转成 JSON 字符串
    ///   3. 写入 PlayerPrefs
    ///   4. PlayerPrefs.Save() 确保立刻写到硬盘
    /// </summary>
    public void Save()
    {
        // ① 包装 List
        ListWrapper wrapper = new ListWrapper { items = items };

        // ② 转成 JSON 字符串
        string json = JsonUtility.ToJson(wrapper);

        // ③ 存入 PlayerPrefs
        PlayerPrefs.SetString("BagData", json);

        // ④ 强制写入硬盘
        // 不调用 Save() 的话，Unity 可能等一会才写，
        // 如果玩家马上关游戏可能丢失数据
        PlayerPrefs.Save();
    }

    // =============================================
    // 内部包装类
    // =============================================

    /// <summary>
    /// 包装类，用于绕开 JsonUtility 的限制。
    ///
    /// 为什么需要这个？
    ///   Unity 的 JsonUtility 不能直接序列化 List<T> 作为顶层对象。
    ///   必须把 List 包在一个 class 里才能序列化。
    ///
    /// 比如：
    ///   JsonUtility.ToJson(list)          → 失败，返回 "{}"
    ///   JsonUtility.ToJson(new ListWrapper { items = list }) → 成功
    /// </summary>
    [System.Serializable]  // 标记为可序列化，JsonUtility 才能处理
    class ListWrapper
    {
        public List<PackageLocalItem> items;
    }
}

/// <summary>
/// ==============================================
/// 背包中的单个物品
/// ==============================================
/// 这不是配置表里的物品定义，而是"玩家拥有的一个具体物品"。
///
/// 举例说明区别：
///   PackageTableItem：  "游戏里有钥匙这种东西，id=1，图标是 key.png"
///   PackageLocalItem：  "玩家背包里有一把钥匙，uid=abc123, id=1, 数量=1"
///
/// uid 的作用：
///   假如玩家有两把同样的钥匙，它们的 id 都是 1，
///   但 uid 不同，这样就能区分"这一把"和"那一把"。
/// </summary>
[System.Serializable]  // 标记为可序列化，这样才能存到硬盘
public class PackageLocalItem
{
    /// <summary> 唯一 ID（GUID），用于区分同一物品的不同实例 </summary>
    public string uid;

    /// <summary> 物品编号，对应 PackageTableItem 里的 id </summary>
    public int id;

    /// <summary> 数量（目前固定为 1，以后可以改成可堆叠） </summary>
    public int num;
}

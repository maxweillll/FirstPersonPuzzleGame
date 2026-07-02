using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ==============================================
/// 游戏总管（单例）
/// ==============================================
/// 作用：这是整个游戏的"大管家"，所有全局操作都通过它。
///
/// 它负责三件事：
///   1. 加载物品配置表（PackageTable.asset）
///   2. 根据 id 查找物品配置
///   3. 管理背包：读取、添加物品（自动存档）
///
/// 怎么用：
///   - 捡起物品：GameManager.Instance.AddToBag(物品id);
///   - 查看背包：GameManager.Instance.GetBagItems();
///   - 查配置：  GameManager.Instance.GetItemById(id);
///
/// 它挂在哪里：
///   场景里随便一个 GameObject 上。设为 DontDestroyOnLoad，
///   切换场景也不会被销毁，整个游戏只有一个。
/// </summary>
public class GameManager : MonoBehaviour
{
    // ---------- 单例 ----------

    /// <summary>
    /// 单例实例。用 GameManager.Instance 就能访问，不需要 Find 或拖引用。
    /// 在 Awake() 里赋值。
    /// </summary>
    public static GameManager Instance;

    // ---------- 缓存 ----------

    /// <summary>
    /// 物品配置表的缓存。第一次用到时才加载，之后直接复用。
    /// 避免每次查表都去读硬盘。
    /// </summary>
    private PackageTable packageTable;

    // ---------- Unity 生命周期 ----------

    private void Awake()
    {
        // 把自己设为单例
        Instance = this;

        // 切换场景时不要销毁这个 GameObject
        // 这样背包数据不会因为切场景而丢失
        DontDestroyOnLoad(gameObject);
    }

    // =============================================
    // 配置表相关
    // =============================================

    /// <summary>
    /// 从 Resources 文件夹加载物品配置表。
    /// 文件位置：Assets/Resources/TableData/PackageTable.asset
    ///
    /// 什么是配置表？
    ///   策划在 Unity 里填的一张表，定义了每个物品的 id、名字、描述、图片路径等。
    ///   代码只读，不改它。
    /// </summary>
    public PackageTable GetPackageTable()
    {
        // 如果还没加载过，就从 Resources 加载
        if (packageTable == null)
            packageTable = Resources.Load<PackageTable>("TableData/PackageTable");
        return packageTable;
    }

    /// <summary>
    /// 根据物品 id 查找它在配置表里的数据。
    ///
    /// 参数：id - 物品编号（比如 1=钥匙, 2=金币）
    /// 返回：配置数据（名字、描述、图片路径等），找不到返回 null
    /// </summary>
    public PackageTableItem GetItemById(int id)
    {
        // 遍历配置表的每一行，找到 id 匹配的那个
        foreach (var item in GetPackageTable().DataList)
            if (item.id == id)
                return item;
        return null;  // 没找到
    }

    // =============================================
    // 背包相关
    // =============================================

    /// <summary>
    /// 获取玩家背包里的所有物品。
    /// 数据来源：PackageLocalData，它会先从内存读，没有再从硬盘读。
    /// </summary>
    public List<PackageLocalItem> GetBagItems()
    {
        return PackageLocalData.Instance.Load();
    }

    /// <summary>
    /// 把一个物品加入背包，并自动保存到硬盘。
    ///
    /// 参数：itemId - 要添加的物品的 id（对应配置表里的 id）
    ///
    /// 举例：
    ///   GameManager.Instance.AddToBag(1);  // 捡起 id=1 的物品（比如钥匙）
    ///
    /// 流程：
    ///   1. 创建一个新的 PackageLocalItem（生成唯一 uid）
    ///   2. 加入背包列表
    ///   3. 立刻保存到 PlayerPrefs（硬盘）
    /// </summary>
    public void AddToBag(int itemId)
    {
        // 创建一个新的物品实例
        PackageLocalItem newItem = new PackageLocalItem();
        newItem.uid = System.Guid.NewGuid().ToString();  // 生成唯一ID，比如 "a3f2b1c4-..."
        newItem.id = itemId;                              // 物品编号
        newItem.num = 1;                                  // 数量=1

        // 加入背包并保存
        PackageLocalData.Instance.Load().Add(newItem);    // 1. 确保已加载，然后加入列表
        PackageLocalData.Instance.Save();                 // 2. 写入硬盘
    }
}

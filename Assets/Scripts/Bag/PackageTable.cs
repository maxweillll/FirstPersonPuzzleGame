using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ==============================================
/// 物品配置表（ScriptableObject）
/// ==============================================
/// 这是一张"静态数据表"，策划在 Unity Editor 里填写。
///
/// 怎么创建？
///   在 Project 窗口右键 → Create → XiaoQi → PackageTable
///   会生成一个 .asset 文件，放在 Resources/TableData/ 下。
///
/// 它和 PackageLocalItem 的区别：
///   PackageTable     = 游戏里"有哪些物品"（策划定义，只读）
///   PackageLocalItem = 玩家"当前拥有什么"（运行时变化，可存储）
///
/// [CreateAssetMenu] 的作用：
///   让你在 Project 窗口右键菜单里能创建这个资产文件。
/// </summary>
[CreateAssetMenu(menuName = "XiaoQi/PackageTable", fileName = "PackageTable")]
public class PackageTable : ScriptableObject
{
    /// <summary> 物品列表，每一行是一个物品的配置 </summary>
    public List<PackageTableItem> DataList = new List<PackageTableItem>();
}

/// <summary>
/// ==============================================
/// 配置表中的一行：一个物品的定义
/// ==============================================
/// 这是策划填的"物品模板"，定义了每个物品长什么样。
///
/// [System.Serializable]：
///   标记后才能在 Unity Inspector 里编辑，才能被 JsonUtility 序列化。
///
/// 字段说明：
///   id         - 物品编号（唯一），比如 1=钥匙, 2=金币
///   type       - 物品类型，1=武器, 2=食物（解密游戏可以不用）
///   star       - 星级（解密游戏可以不用）
///   name       - 物品名称，比如"金色钥匙"
///   description- 物品描述，比如"一把闪闪发光的钥匙"
///   imagePath  - 图标路径，比如 "Icons/key"（对应 Resources/Icons/key.png）
/// </summary>
[System.Serializable]
public class PackageTableItem
{
    public int id;                // 物品编号
    public int type;              // 类型
    public string name;           // 物品名称
    public string description;    // 物品描述
    public string imagePath;      // 图标在 Resources 下的路径
}

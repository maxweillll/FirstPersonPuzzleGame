using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

/// <summary>
/// GM 命令（只在 Editor 下可用）。
/// 菜单栏 CMcommand → 读取配置表 / 写入背包 / 读取背包
/// </summary>
public class GMcommand
{
    [MenuItem("CMcommand/读取配置表")]
    public static void ReadTable()
    {
        PackageTable packageTable = Resources.Load<PackageTable>("TableData/PackageTable");
        foreach (PackageTableItem packageTableItem in packageTable.DataList)
        {
            Debug.Log(string.Format("[id]: {0}, [name]: {1}", packageTableItem.id, packageTableItem.name));
        }
    }

    [MenuItem("CMcommand/写入背包数据（测试用）")]
    public static void WriteLocalPackageData()
    {
        // 直接操作 Load() 返回的列表：先清空，再添加 9 个测试物品
        List<PackageLocalItem> items = PackageLocalData.Instance.Load();
        items.Clear();

        for (int i = 0; i < 9; i++)
        {
            PackageLocalItem packageLocalItem = new PackageLocalItem();
            packageLocalItem.uid = System.Guid.NewGuid().ToString();
            packageLocalItem.id = i + 1;
            packageLocalItem.num = Random.Range(1, 10);
            items.Add(packageLocalItem);
        }

        PackageLocalData.Instance.Save();
        Debug.Log("背包测试数据已写入");
    }

    [MenuItem("CMcommand/读取背包数据")]
    public static void ReadLocalPackageData()
    {
        List<PackageLocalItem> items = PackageLocalData.Instance.Load();
        foreach (PackageLocalItem packageLocalItem in items)
        {
            Debug.Log(string.Format("[id]:{0} [num]:{1}", packageLocalItem.id, packageLocalItem.num));
        }
    }
}

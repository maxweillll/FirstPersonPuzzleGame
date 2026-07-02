using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ==============================================
/// 背包物品格子
/// ==============================================
/// 挂载在每个物品格子的预制体（PackgeUiItem.prefab）上。
///
/// 每个格子负责：
///   1. 显示物品的图标（SetData）
///   2. 被点击时通知详情面板（OnClick）
///
/// 预制体需要什么？
///   - 一个 Button 组件（用于点击）
///   - 一个 Image 子物体（用于显示图标）
///   - 把 Image 拖到 icon 字段上
///   - 把 Button 的 OnClick 事件绑定到 OnClick() 方法
/// </summary>
public class PackageCell : MonoBehaviour
{
    /// <summary>
    /// 物品图标 Image 组件。
    /// 在 Inspector 里从子物体拖过来。
    /// </summary>
    public Image icon;

    /// <summary>
    /// 这个格子对应的物品数据。
    /// SetData() 时赋值，OnClick() 时使用。
    /// </summary>
    private PackageLocalItem data;

    /// <summary>
    /// 填入物品数据，并显示图标。
    ///
    /// 流程：
    ///   ① 保存数据引用
    ///   ② 根据 data.id 查配置表（获取图片路径）
    ///   ③ 用 Resources.Load 加载图片
    ///   ④ 把图片设为 Image 的 sprite（图标）
    ///
    /// 参数：item - 这个格子对应的物品数据
    /// </summary>
    public void SetData(PackageLocalItem item)
    {
        // ① 保存数据，点击时会用到
        data = item;

        // ② 查配置表：根据 id 获取物品的名字、描述、图片路径等
        PackageTableItem config = GameManager.Instance.GetItemById(item.id);
        if (config == null) return;  // 配置不存在就退出

        // ③ 从 Resources 加载图片
        // config.imagePath 是配置表里填的路径，比如 "Icons/key"
        // Resources.Load 会去找 Assets/Resources/Icons/key.png
        Texture2D tex = Resources.Load<Texture2D>(config.imagePath);

        // ④ 把纹理转成 Sprite，设为图标
        if (tex != null && icon != null)
        {
            // Sprite.Create: 从纹理创建 Sprite
            // Rect(0, 0, tex.width, tex.height) = 使用整张图
            // Vector2.zero = 锚点在左下角
            icon.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }
    }

    /// <summary>
    /// 点击格子时触发。
    /// 找到场景里的 PackageDetail（详情面板），让它显示这个物品的信息。
    ///
    /// 注意：这个方法需要在 Button 组件的 OnClick 事件列表里绑定！
    /// 在 Unity Editor 里：选中预制体 → Button 组件 → OnClick → 拖自己 → 选 PackageCell.OnClick
    /// </summary>
    public void OnClick()
    {
        // 没有数据就不处理
        if (data == null) return;

        // 先尝试从父物体找详情面板（如果它在同一个预制体内）
        PackageDetail detail = GetComponentInParent<PackageDetail>();

        // 找不到就用 FindObjectOfType 全局找（跨预制体）
        if (detail == null)
            detail = FindObjectOfType<PackageDetail>();

        // 找到了就显示详情
        if (detail != null)
            detail.Show(data);
    }
}

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ==============================================
/// 物品详情面板
/// ==============================================
/// 点击背包里的物品格子后，这个面板显示物品的详细信息。
///
/// 显示内容：
///   - 物品名称（大标题）
///   - 物品描述（说明文字）
///   - 物品大图
///
/// 你在 Inspector 里需要拖好三个引用：
///   titleText       → 标题 Text 组件
///   descriptionText → 描述 Text 组件
///   iconImage       → 大图 Image 组件
/// </summary>
public class PackageDetail : MonoBehaviour
{
    /// <summary> 物品名称（比如"金色钥匙"）</summary>
    public Text titleText;

    /// <summary> 物品描述（比如"一把闪闪发光的钥匙，可以打开地下室的门。"）</summary>
    public Text descriptionText;

    /// <summary> 物品的大图 </summary>
    public Image iconImage;

    /// <summary>
    /// 显示一个物品的详细信息。
    ///
    /// 流程：
    ///   ① 根据 item.id 查配置表
    ///   ② 把配置的名字、描述、图片填到 UI 上
    ///
    /// 参数：item - 玩家点击的那个物品的数据
    /// </summary>
    public void Show(PackageLocalItem item)
    {
        // ① 查配置表
        PackageTableItem config = GameManager.Instance.GetItemById(item.id);
        if (config == null) return;  // 配置不存在，不显示

        // ② 填数据到 UI
        // 每个字段都判空，防止没拖引用导致崩溃
        if (titleText != null)
            titleText.text = config.name;           // 显示名字

        if (descriptionText != null)
            descriptionText.text = config.description;  // 显示描述

        if (iconImage != null)
        {
            // 加载大图
            Texture2D tex = Resources.Load<Texture2D>(config.imagePath);
            if (tex != null)
                iconImage.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }
    }
}

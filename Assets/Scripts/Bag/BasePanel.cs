using UnityEngine;

/// <summary>
/// ==============================================
/// 面板基类
/// ==============================================
/// 所有 UI 面板（背包、主菜单、设置等）都继承这个类。
///
/// 它只做两件事：
///   Open()  - 显示面板（SetActive(true)）
///   Close() - 隐藏面板 + 通知 UIManager 注销 + 销毁 GameObject
///
/// 为什么要有基类？
///   这样 UIManager 不需要知道每个面板具体是什么类型，
///   统一用 BasePanel 类型来管理就行了。
///
/// 怎么用（新建一个面板）：
///   public class 我的面板 : BasePanel
///   {
///       // 你的代码...
///   }
/// </summary>
public class BasePanel : MonoBehaviour
{
    /// <summary>
    /// 打开面板：让这个 GameObject 显示出来。
    /// SetActive(true) 会激活物体及其所有子物体。
    /// </summary>
    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// 关闭面板：
    ///   1. SetActive(false) 隐藏
    ///   2. 告诉 UIManager "我关了"，它从已打开列表里移除
    ///   3. Destroy(gameObject) 销毁自己（释放内存）
    ///
    /// 标记为 virtual：子类可以重写，比如关闭前播放动画。
    /// </summary>
    public virtual void Close()
    {
        gameObject.SetActive(false);                  // 隐藏
        UIManager.Instance.OnPanelClosed(name);       // 从管理器注销
        Destroy(gameObject);                          // 销毁
    }
}

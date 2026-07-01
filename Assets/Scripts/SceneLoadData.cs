// 场景切换时传递数据的静态类
// 不需要挂到 GameObject 上，其他脚本直接 SceneLoadData.TargetSceneName 读写
public static class SceneLoadData
{
    public static string TargetSceneName;
    public static int TargetSceneIndex = -1;
    public static bool UseIndex = false;
}

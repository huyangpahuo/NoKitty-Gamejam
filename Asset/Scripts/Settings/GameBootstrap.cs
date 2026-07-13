using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private void Awake()
    {
        InitializeSave();
    }

    /// <summary>
    /// 游戏启动时初始化存档
    /// </summary>
    private void InitializeSave()
    {
        SettingsData data = SettingsManager.Load();

        if (data == null)
        {
            Debug.Log("未检测到存档，创建默认存档");

            data = new SettingsData();

            SettingsManager.Save(data);
        }
        else
        {
            Debug.Log("已加载存档");
        }
    }
}
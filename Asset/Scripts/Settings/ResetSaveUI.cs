using UnityEngine;

public class ResetSaveUI : MonoBehaviour
{
    [Header("确认面板")]
    [SerializeField] private GameObject confirmPanel;

    private void Awake()
    {
        // 默认隐藏确认窗口
        confirmPanel.SetActive(false);
    }

    /// <summary>
    /// 点击“重置存档”按钮
    /// </summary>
    public void ShowConfirm()
    {
        confirmPanel.SetActive(true);
    }

    /// <summary>
    /// 点击“我再想想”
    /// </summary>
    public void CancelReset()
    {
        confirmPanel.SetActive(false);
    }

    /// <summary>
    /// 点击“确定重置”
    /// </summary>
    public void ConfirmReset()
    {
        // 删除存档文件
        SettingsManager.DeleteSave();

        FindFirstObjectByType<SettingsUI>()?.ResetRuntimeData();

        // 关键：不要调用 Load()
        // 因为 Load 不再负责创建存档

        confirmPanel.SetActive(false);

        Debug.Log("存档已彻底重置（不会自动恢复）");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [Header("设置总面板")]
    [SerializeField] private GameObject settingsPanel;

    [Header("所有页面")]
    [SerializeField] private GameObject[] pages;

    [Header("默认打开页面")]
    [SerializeField] private int defaultPage = 0;

    private bool isOpen;
    private int currentPage;

    [Header("Volume")]

    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider masterSlider;

    [SerializeField] private TMP_Text bgmText;
    [SerializeField] private TMP_Text sfxText;
    [SerializeField] private TMP_Text masterText;

    private SettingsData settingsData;

    private void Start()
    {
        if (settingsPanel == null)
        {
            Debug.LogError("SettingsPanel 未指定！");
            return;
        }

        settingsPanel.SetActive(false);

        if (pages != null && pages.Length > 0)
        {
            defaultPage = Mathf.Clamp(defaultPage, 0, pages.Length - 1);

            currentPage = defaultPage;

            ShowPage(currentPage);
        }

        // 读取配置
        settingsData = SettingsManager.Load();

        // JSON存储0~1
        // Slider显示0~100
        bgmSlider.value = settingsData.bgmVolume * 100f;
        sfxSlider.value = settingsData.sfxVolume * 100f;
        masterSlider.value = settingsData.masterVolume * 100f;

        UpdateTexts();

        bgmSlider.onValueChanged.AddListener(OnBGMChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        masterSlider.onValueChanged.AddListener(OnMasterChanged);

        Debug.Log(SettingsManager.GetSavePath());
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleSettings();
        }
    }

    /// <summary>
    /// 退出游戏时再保存一次，防止意外丢失
    /// </summary>
    private void OnApplicationQuit()
    {
        if (settingsData != null)
        {
            SettingsManager.Save(settingsData);
        }
    }

    /// <summary>
    /// 重置运行时数据（防止删除存档后被旧数据写回）
    /// </summary>
    public void ResetRuntimeData()
    {
        settingsData = new SettingsData();

        bgmSlider.value = 100;
        sfxSlider.value = 100;
        masterSlider.value = 60;

        UpdateTexts();
    }

    /// <summary>
    /// 主音量
    /// </summary>
    private void OnMasterChanged(float value)
    {
        masterText.text = Mathf.RoundToInt(value).ToString();

        // 转成0~1存储
        settingsData.masterVolume = value / 100f;

        ApplyAndSave();
    }

    /// <summary>
    /// 音效
    /// </summary>
    private void OnSFXChanged(float value)
    {
        sfxText.text = Mathf.RoundToInt(value).ToString();

        settingsData.sfxVolume = value / 100f;

        ApplyAndSave();
    }

    /// <summary>
    /// 背景音乐
    /// </summary>
    private void OnBGMChanged(float value)
    {
        bgmText.text = Mathf.RoundToInt(value).ToString();

        settingsData.bgmVolume = value / 100f;

        ApplyAndSave();
    }

    /// <summary>
    /// 立即应用音量（SFX + BGM）并实时保存到磁盘
    /// </summary>
    private void ApplyAndSave()
    {
        // 用内存中的最新数据直接应用，不依赖磁盘上可能还没保存的旧值
        AudioManager.Instance.ApplyVolume(settingsData);
        BGMController.ApplyVolume(settingsData);

        SettingsManager.Save(settingsData);
    }

    /// <summary>
    /// 更新数字
    /// </summary>
    private void UpdateTexts()
    {
        bgmText.text = Mathf.RoundToInt(bgmSlider.value).ToString();

        sfxText.text = Mathf.RoundToInt(sfxSlider.value).ToString();

        masterText.text = Mathf.RoundToInt(masterSlider.value).ToString();
    }

    public void OpenByButton()
    {
        OpenSettings();
    }

    public void OpenSettings()
    {
        if (settingsPanel == null)
            return;

        isOpen = true;

        settingsPanel.SetActive(true);

        ShowPage(defaultPage);

        Time.timeScale = 0f;
    }

    public void CloseSettings()
    {
        if (settingsPanel == null)
            return;

        isOpen = false;

        settingsPanel.SetActive(false);

        Time.timeScale = 1f;
    }

    public void PlayUISound()
    {
        AudioManager.Instance.PlaySFX("UIButtonClickClip");
    }

    public void ToggleSettings()
    {
        PlayUISound();

        if (isOpen)
            CloseSettings();
        else
            OpenSettings();
    }

    public void ShowPage(int pageIndex)
    {
        if (pages == null || pages.Length == 0)
            return;

        pageIndex = Mathf.Clamp(pageIndex, 0, pages.Length - 1);

        currentPage = pageIndex;

        for (int i = 0; i < pages.Length; i++)
        {
            if (pages[i] != null)
            {
                pages[i].SetActive(i == currentPage);
            }
        }
    }

    public void NextPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        currentPage++;

        if (currentPage >= pages.Length)
            currentPage = 0;

        ShowPage(currentPage);
    }

    public void PreviousPage()
    {
        if (pages == null || pages.Length == 0)
            return;

        currentPage--;

        if (currentPage < 0)
            currentPage = pages.Length - 1;

        ShowPage(currentPage);
    }

    public bool IsOpen()
    {
        return isOpen;
    }

    public int GetCurrentPage()
    {
        return currentPage;
    }
}
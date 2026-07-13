using UnityEngine;

public class BGMController : MonoBehaviour
{
    public static bool isClone;

    public GameObject BGM;

    public static AudioSource bgmSource;

    private GameObject obj;

    void Awake()
    {
        if (!isClone)
        {
            obj = Instantiate(BGM, Vector3.zero, Quaternion.identity);
            DontDestroyOnLoad(obj);

            bgmSource = obj.GetComponent<AudioSource>();

            isClone = true;
        }

        // 每次场景加载都重新应用一次音量（用最新保存的设置）
        ApplyVolume(SettingsManager.Load());
    }

    /// <summary>
    /// 用传入的SettingsData应用BGM音量
    /// 设置面板滑动条拖动时应调用这个版本
    /// </summary>
    public static void ApplyVolume(SettingsData data)
    {
        if (bgmSource == null || data == null)
            return;

        bgmSource.volume = data.masterVolume * data.bgmVolume;
    }
}
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SFXEntry
{
    public string id;
    public AudioSource prefab;
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("场景内预先指定的音效（可选，仍支持按id播放）")]
    [SerializeField] private List<SFXEntry> sfxEntries;

    private Dictionary<string, AudioSource> sfxLookup;
    private Dictionary<AudioSource, AudioSource> prefabToInstance;

    private void Awake()
    {
        Instance = this;

        sfxLookup = new Dictionary<string, AudioSource>();
        prefabToInstance = new Dictionary<AudioSource, AudioSource>();

        foreach (var entry in sfxEntries)
        {
            if (entry.prefab == null) continue;

            AudioSource instance = GetOrCreateInstance(entry.prefab);

            if (!sfxLookup.ContainsKey(entry.id))
                sfxLookup.Add(entry.id, instance);
        }
    }

    private void Start()
    {
        ApplyVolume();
    }

    /// <summary>
    /// 按id播放（用于代码里已经注册好的音效）
    /// </summary>
    public void PlaySFX(string id)
    {
        if (sfxLookup.TryGetValue(id, out AudioSource source))
        {
            source.Play();
        }
        else
        {
            Debug.LogWarning($"AudioManager: 未找到音效 id = {id}");
        }
    }

    /// <summary>
    /// 直接传入音效Prefab播放（用于按钮脚本等直接拖拽的场景）
    /// </summary>
    public void PlaySFX(AudioSource prefab)
    {
        if (prefab == null) return;

        AudioSource instance = GetOrCreateInstance(prefab);
        instance.Play();
    }
    /// <summary>
    /// 停止播放（如果正在播放的话）
    /// </summary>
    public void StopSFX(AudioSource prefab)
    {
        if (prefab == null) return;

        if (prefabToInstance.TryGetValue(prefab, out AudioSource instance))
        {
            instance.Stop();
        }
    }

    private AudioSource GetOrCreateInstance(AudioSource prefab)
    {
        if (prefabToInstance.TryGetValue(prefab, out AudioSource existing))
            return existing;

        AudioSource instance = Instantiate(prefab, transform);
        instance.gameObject.name = prefab.name;

        SettingsData data = SettingsManager.Load();
        instance.volume = data.masterVolume * data.sfxVolume;

        prefabToInstance.Add(prefab, instance);

        return instance;
    }

    /// <summary>
    /// 从磁盘读取设置并应用（场景启动等没有现成SettingsData时使用）
    /// </summary>
    public void ApplyVolume()
    {
        ApplyVolume(SettingsManager.Load());
    }

    /// <summary>
    /// 直接用传入的SettingsData应用音量（避免依赖磁盘上可能还没保存的旧值）
    /// 滑动条拖动时应调用这个版本，而不是无参版本
    /// </summary>
    public void ApplyVolume(SettingsData data)
    {
        if (data == null) return;

        foreach (var source in prefabToInstance.Values)
        {
            if (source != null)
                source.volume = data.masterVolume * data.sfxVolume;
        }
    }
}
using UnityEngine;
using System.IO;

public static class SettingsManager
{
    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "settings.json");

    /// <summary>
    /// 读取存档（只读，不创建）
    /// </summary>
    public static SettingsData Load()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                // 没有存档就返回 null，不自动创建
                return null;
            }

            string json = File.ReadAllText(SavePath);
            SettingsData data = JsonUtility.FromJson<SettingsData>(json);

            return data;
        }
        catch
        {
            Debug.LogWarning("Settings文件损坏，返回空数据");

            return null;
        }
    }

    /// <summary>
    /// 保存存档
    /// </summary>
    public static void Save(SettingsData data)
    {
        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"保存设置失败: {e}");
        }
    }

    /// <summary>
    /// 删除存档文件
    /// </summary>
    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("存档已删除");
        }
    }


    public static string GetSavePath()
    {
        return SavePath;
    }

    // ========================
    // 以下逻辑保持不变
    // ========================

    public static bool HasPlayedStory(string storyId)
    {
        SettingsData data = Load();
        return data != null && data.playedStories.Contains(storyId);
    }

    public static void MarkStoryPlayed(string storyId)
    {
        SettingsData data = Load();

        if (data == null)
            data = new SettingsData();

        if (!data.playedStories.Contains(storyId))
        {
            data.playedStories.Add(storyId);
            Save(data);
        }
    }

    public static bool IsLevelUnlocked(string levelId)
    {
        SettingsData data = Load();
        return data != null && data.IsLevelUnlocked(levelId);
    }

    public static void UnlockLevel(string levelId)
    {
        SettingsData data = Load();

        if (data == null)
            data = new SettingsData();

        if (!data.unlockedLevels.Contains(levelId))
        {
            data.unlockedLevels.Add(levelId);
            Save(data);
        }
    }

    public static bool IsPickupCollected(string pickupId)
    {
        SettingsData data = Load();
        return data != null && data.collectedPickupIds.Contains(pickupId);
    }

    public static void MarkPickupCollected(string pickupId)
    {
        SettingsData data = Load();

        if (data == null)
            data = new SettingsData();

        if (!data.collectedPickupIds.Contains(pickupId))
        {
            data.collectedPickupIds.Add(pickupId);
            Save(data);
        }
    }
}
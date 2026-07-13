using System.Collections.Generic;

[System.Serializable]
public class SettingsData
{
    public float masterVolume = 0.6f;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;

    public List<string> playedStories = new();
    public List<InventorySaveData> inventory = new();

    // 已通关关卡
    public List<string> unlockedLevels = new();

    // 已拾取的世界物品（按 pickupId 记录，防止重新进入场景时复活）
    public List<string> collectedPickupIds = new();

    public bool HasPlayedStory(string storyId)
    {
        return playedStories != null &&
               playedStories.Contains(storyId);
    }

    public bool IsLevelUnlocked(string levelId)
    {
        return unlockedLevels != null &&
               unlockedLevels.Contains(levelId);
    }


}
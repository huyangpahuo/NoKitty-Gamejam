using UnityEngine;

public class StoryItemInteractTrigger : MonoBehaviour
{
    [Header("直接拖入剧情数据（不再手填ID）")]
    [SerializeField] private StoryData storyData;

    private bool HasStory => storyData != null && !string.IsNullOrEmpty(storyData.storyId);

    // 返回 true 表示本次交互已被剧情消耗
    public bool TryPlayStory()
    {
        if (!HasStory)
            return false;

        string id = storyData.storyId;

        if (SettingsManager.HasPlayedStory(id))
            return false;

        if (StoryManager.Instance == null)
            return false;

        StoryManager.Instance.PlayStory(id);
        return true;
    }
}
using UnityEngine;
using System.Text;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance;

    [Header("StoryPlayer预制体")]
    [SerializeField] private StoryPlayer storyPlayerPrefab;

    [Header("StoryPlayer生成父节点")]
    [SerializeField] private Transform storyPlayerParent;

    [Header("全部剧情")]
    [SerializeField] private StoryEntry[] stories;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void PlayStory(string storyId, System.Action onFinished = null)
    {
        StoryEntry entry = FindStory(storyId);
        if (entry == null) return; // FindStory里已打印详细错误

        if (SettingsManager.HasPlayedStory(storyId))
        {
            if (entry.storyRoot != null)
                entry.storyRoot.SetActive(false);

            onFinished?.Invoke();
            return;
        }

        if (entry.storyRoot != null)
            entry.storyRoot.SetActive(true);

        StoryPlayer player = Instantiate(storyPlayerPrefab, storyPlayerParent, false);

        player.Play(
            entry.storyData,
            entry.storyImage,
            entry.storyText,
            () =>
            {
                SettingsManager.MarkStoryPlayed(entry.storyData.storyId);

                if (entry.closeOnEnd != null)
                {
                    foreach (GameObject obj in entry.closeOnEnd)
                    {
                        if (obj != null)
                            obj.SetActive(false);
                    }
                }

                if (entry.storyRoot != null)
                    entry.storyRoot.SetActive(false);

                // 通知外部剧情结束
                onFinished?.Invoke();
            }
        );
    }

    private void ActivateChainTo(Transform child, Transform root)
    {
        if (child == null || root == null) return;

        Transform current = child;
        while (current != null)
        {
            if (!current.gameObject.activeSelf)
                current.gameObject.SetActive(true);

            if (current == root) break;
            current = current.parent;
        }
    }

    public void PlayStoryForce(string storyId, System.Action onFinished = null)
    {
        StoryEntry entry = FindStory(storyId);
        if (entry == null) return;

        if (entry.storyRoot != null)
            entry.storyRoot.SetActive(true);

        if (entry.storyImage != null)
            ActivateChainTo(entry.storyImage.transform, entry.storyRoot.transform);

        if (entry.storyText != null)
            ActivateChainTo(entry.storyText.transform, entry.storyRoot.transform);

        StoryPlayer player = Instantiate(storyPlayerPrefab, storyPlayerParent, false);

        player.Play(
            entry.storyData,
            entry.storyImage,
            entry.storyText,
            () =>
            {
                if (entry.closeOnEnd != null)
                {
                    foreach (GameObject obj in entry.closeOnEnd)
                    {
                        if (obj != null)
                            obj.SetActive(false);
                    }
                }

                if (entry.storyRoot != null)
                    entry.storyRoot.SetActive(false);

                // 通知外部剧情结束
                onFinished?.Invoke();
            }
        );
    }

    private StoryEntry FindStory(string storyId)
    {
        foreach (StoryEntry entry in stories)
        {
            if (entry == null || entry.storyData == null) continue;
            if (entry.storyData.storyId == storyId) return entry;
        }

        // 详细错误：输出当前可用ID
        StringBuilder sb = new StringBuilder();
        sb.Append("[StoryManager] Available storyIds: ");
        bool any = false;

        foreach (StoryEntry entry in stories)
        {
            if (entry == null || entry.storyData == null) continue;
            if (any) sb.Append(", ");
            sb.Append(entry.storyData.storyId);
            any = true;
        }

        if (!any) sb.Append("(none)");

        Debug.LogError($"未找到剧情: {storyId}\n{sb}");
        return null;
    }
}
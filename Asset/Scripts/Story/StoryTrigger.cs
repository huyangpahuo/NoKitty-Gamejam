using UnityEngine;

public class StoryTrigger : MonoBehaviour
{
    [Header("剧情")]
    [SerializeField] private StoryData storyData;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player"))
            return;

        if (storyData == null)
        {
            Debug.LogError($"{name} 没有指定 StoryData！");
            return;
        }

        triggered = true;

        StoryManager.Instance.PlayStory(storyData.storyId, OnStoryFinished);
    }

    private void OnStoryFinished()
    {
        gameObject.SetActive(false);
    }
}
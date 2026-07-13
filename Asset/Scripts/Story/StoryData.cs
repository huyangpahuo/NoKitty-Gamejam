using UnityEngine;

[CreateAssetMenu(fileName = "StoryData", menuName = "Story/Story Data")]
public class StoryData : ScriptableObject
{
    [Header("剧情唯一ID")]
    public string storyId;

    public StoryNode[] nodes;
}
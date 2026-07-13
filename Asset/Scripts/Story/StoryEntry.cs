using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class StoryEntry
{
    [Header("剧情数据")]
    public StoryData storyData;

    [Header("剧情根UI")]
    public GameObject storyRoot;

    [Header("图片")]
    public Image storyImage;

    [Header("文字")]
    public TMP_Text storyText;

    [Header("结束后关闭")]
    public GameObject[] closeOnEnd;
}
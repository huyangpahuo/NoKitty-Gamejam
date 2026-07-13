using UnityEngine;

[System.Serializable]
public class StoryNode
{
    [Header("画面")]
    public Sprite image;

    [TextArea(3, 10)]
    public string text;

    [Header("自动播放（秒）")]
    public float autoNextTime = 0f;

    [Header("是否自动播放")]
    public bool autoNext = false;
}
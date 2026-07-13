using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Item")]
public class ItemData : ScriptableObject
{
    [Header("物品唯一ID")]
    public string itemID;

    [Header("基础信息")]
    public string itemName;

    [TextArea(3, 10)]
    public string description;

    [Header("图标")]
    public Sprite icon;

    [Header("堆叠上限")]
    public int maxStack = 999;
}
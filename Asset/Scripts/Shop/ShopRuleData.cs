using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShopRule", menuName = "Game/Shop Rule")]
public class ShopRuleData : ScriptableObject
{
    [Header("【基础】货币与折扣配置")]
    [Tooltip("交易使用的货币物品（例如：金币）")]
    public ItemData currencyItem;

    [Tooltip("优惠券物品（可为空）。当背包有此物品时启用折扣")]
    public ItemData couponItem;

    [Tooltip("折扣倍率（0.5=五折）")]
    [Range(0.01f, 1f)] public float discountMultiplier = 0.5f;

    [Header("【购买列表】玩家可从商店购买")]
    public List<ShopBuyEntry> buyList = new();

    [Header("【出售列表】玩家可向商店出售")]
    public List<ShopSellEntry> sellList = new();
}

[System.Serializable]
public class ShopBuyEntry
{
    [Header("【购买项】商品配置")]
    [Tooltip("商品物品")]
    public ItemData item;

    [Tooltip("每次购买给玩家的数量")]
    public int giveAmount = 1;

    [Tooltip("每次购买消耗的货币数量")]
    public int priceCurrency = 1;

    [Tooltip("库存数量：-1 表示无限库存，0 表示售罄")]
    public int stock = -1;
}

[System.Serializable]
public class ShopSellEntry
{
    [Header("【出售项】回收配置")]
    [Tooltip("可出售给商店的物品")]
    public ItemData item;

    [Tooltip("每次出售需要扣除玩家该物品数量")]
    public int takeAmount = 1;

    [Tooltip("每次出售返还给玩家的货币数量")]
    public int rewardCurrency = 1;
}
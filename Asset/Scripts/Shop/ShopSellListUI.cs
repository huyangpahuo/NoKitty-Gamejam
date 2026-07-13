using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 右侧“可出售物品列表”显示控制：
/// - 读取当前商店规则 sellList
/// - 读取玩家背包数量
/// - 只显示：在规则里允许卖 且 背包数量>0 的物品
/// - 使用你现有 InventorySlotUI 预制体显示（仅显示，不改其逻辑）
/// - 支持点击选中某一格用于出售（不影响其拖拽到合成栏的功能）
/// </summary>
public class ShopSellListUI : MonoBehaviour
{
    [Header("【UI】右侧列表容器（ScrollView/Content）")]
    [SerializeField] private Transform sellContent;

    [Header("【UI】复用背包格子预制体（InventorySlotUI）")]
    [SerializeField] private InventorySlotUI slotPrefab;

    private readonly List<InventorySlotUI> _spawned = new();
    private string _selectedItemId;

    /// <summary>Fired when the selected sell item changes (null = nothing selected).</summary>
    public event Action<ItemData> OnItemSelected;

    public void Refresh(ShopRuleData rule)
    {
        ClearAll();

        if (rule == null || rule.sellList == null) return;
        var inv = InventoryManager.Instance;
        if (inv == null) return;
        if (sellContent == null || slotPrefab == null) return;

        bool selectionStillPresent = string.IsNullOrEmpty(_selectedItemId);

        foreach (var entry in rule.sellList)
        {
            if (entry == null || entry.item == null) continue;

            int owned = inv.GetTotalAmount(entry.item.itemID);
            if (owned <= 0) continue; // 只显示背包里有的可卖物品

            var slot = Instantiate(slotPrefab, sellContent);
            slot.SetData(entry.item, owned);
            slot.Clicked += HandleSlotClicked;

            bool isSelected = entry.item.itemID == _selectedItemId;
            slot.SetSelected(isSelected);
            if (isSelected) selectionStillPresent = true;

            _spawned.Add(slot);
        }

        // If the previously selected item is no longer sellable (e.g. all sold), clear selection.
        if (!selectionStillPresent)
        {
            _selectedItemId = null;
            OnItemSelected?.Invoke(null);
        }
    }

    public void ClearAll()
    {
        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] != null)
            {
                _spawned[i].Clicked -= HandleSlotClicked;
                Destroy(_spawned[i].gameObject);
            }
        }
        _spawned.Clear();
    }

    /// <summary>Fully clears the current selection (call when the shop panel closes).</summary>
    public void ResetSelection()
    {
        _selectedItemId = null;
    }

    private void HandleSlotClicked(InventorySlotUI slot)
    {
        var item = slot.CurrentItem;
        if (item == null) return;

        // Clicking the already-selected slot deselects it.
        if (_selectedItemId == item.itemID)
        {
            _selectedItemId = null;
            slot.SetSelected(false);
            OnItemSelected?.Invoke(null);
            return;
        }

        _selectedItemId = item.itemID;

        for (int i = 0; i < _spawned.Count; i++)
        {
            if (_spawned[i] != null)
                _spawned[i].SetSelected(_spawned[i].CurrentItem == item);
        }

        OnItemSelected?.Invoke(item);
    }
}
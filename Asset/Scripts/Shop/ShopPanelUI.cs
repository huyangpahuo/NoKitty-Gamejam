using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelUI : MonoBehaviour
{
    [Header("【面板】根节点")]
    [SerializeField] private GameObject panelRoot;

    [Header("【金钱显示】图标与数字")]
    [SerializeField] private Image moneyIcon;
    [SerializeField] private AnimatedNumberText moneyNumber;

    [Header("【商品列表】容器与预制体")]
    [SerializeField] private Transform buyContent;
    [SerializeField] private ShopBuyItemSlotUI buySlotPrefab;

    [Header("【购买操作】购买按钮")]
    [SerializeField] private Button buyButton;

    [Header("【出售列表】右侧可售卖物品显示")]
    [SerializeField] private ShopSellListUI sellListUI;

    [Header("【出售操作】出售按钮")]
    [SerializeField] private Button sellButton;

    private ItemData _selectedSellItem;

    private ShopRuleData _rule;
    private readonly List<ShopBuyItemSlotUI> _slots = new();
    private ShopBuyItemSlotUI _selected;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;
    public ShopRuleData CurrentRule => _rule;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        var inv = InventoryManager.Instance;
        if (inv != null) inv.OnInventoryChanged += RefreshAll;

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnClickBuy);
            buyButton.interactable = false;
        }

        if (sellListUI != null)
            sellListUI.OnItemSelected += HandleSellItemSelected;

        if (sellButton != null)
        {
            sellButton.onClick.AddListener(OnClickSell);
            sellButton.interactable = false;
        }
    }

    private void OnDisable()
    {
        var inv = InventoryManager.Instance;
        if (inv != null) inv.OnInventoryChanged -= RefreshAll;

        if (buyButton != null) buyButton.onClick.RemoveListener(OnClickBuy);

        if (sellListUI != null)
            sellListUI.OnItemSelected -= HandleSellItemSelected;

        if (sellButton != null) sellButton.onClick.RemoveListener(OnClickSell);
    }

    // 由外部商店触发器调用
    public void OpenWithRule(ShopRuleData rule)
    {
        _rule = rule;
        RebuildBuySlots();
        if (panelRoot != null) panelRoot.SetActive(true);
        RefreshAll();
    }

    // 给关闭按钮 OnClick 用
    public void ClosePanel()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        _selected = null;

        _selectedSellItem = null;
        RefreshSellButtonState();

        if (sellListUI != null)
        {
            sellListUI.ClearAll();
            sellListUI.ResetSelection();
        }
    }

    public void ToggleWithRule(ShopRuleData rule)
    {
        if (IsOpen) ClosePanel();
        else OpenWithRule(rule);
    }

    public void OnSelectSlot(ShopBuyItemSlotUI slot)
    {
        // 再点同一个 -> 取消选中
        if (_selected == slot)
            _selected = null;
        else
            _selected = slot;

        for (int i = 0; i < _slots.Count; i++)
            _slots[i].SetSelected(_slots[i] == _selected);

        RefreshBuyButtonState();
    }

    // 出售列表格子被点击选中/取消选中时调用（由 ShopSellListUI 的事件驱动）
    private void HandleSellItemSelected(ItemData item)
    {
        _selectedSellItem = item;
        RefreshSellButtonState();
    }

    private void RefreshSellButtonState()
    {
        if (sellButton == null) return;
        sellButton.interactable = IsOpen && _selectedSellItem != null;
    }

    // 出售按钮点击：卖掉当前点选中的物品（拖拽出售路径 TrySellByItemId 不受影响）
    private void OnClickSell()
    {
        if (_selectedSellItem == null) return;
        TrySellByItemId(_selectedSellItem.itemID);
    }

    public bool TrySellByItemId(string itemId)
    {
        if (!IsOpen || _rule == null || _rule.currencyItem == null) return false;
        if (string.IsNullOrEmpty(itemId)) return false;

        var inv = InventoryManager.Instance;
        if (inv == null) return false;

        var sell = _rule.sellList?.FirstOrDefault(x => x != null && x.item != null && x.item.itemID == itemId);
        if (sell == null) return false;

        int need = Mathf.Max(1, sell.takeAmount);
        int reward = Mathf.Max(1, sell.rewardCurrency);

        if (inv.GetTotalAmount(itemId) < need) return false;
        if (inv.RemoveItem(itemId, need) < need) return false;

        inv.AddItem(_rule.currencyItem.itemID, reward);
        RefreshAll();
        return true;
    }

    private void RebuildBuySlots()
    {
        for (int i = 0; i < _slots.Count; i++)
            if (_slots[i] != null) Destroy(_slots[i].gameObject);
        _slots.Clear();
        _selected = null;

        if (_rule == null || buyContent == null || buySlotPrefab == null || _rule.buyList == null) return;

        foreach (var e in _rule.buyList)
        {
            if (e == null || e.item == null) continue;
            var slot = Instantiate(buySlotPrefab, buyContent);
            slot.Bind(e, this);
            _slots.Add(slot);
        }
    }

    private void RefreshAll()
    {
        if (_rule == null) return;

        var inv = InventoryManager.Instance;
        int money = GetMoneyAmount(inv);

        if (moneyIcon != null)
        {
            moneyIcon.sprite = _rule.currencyItem != null ? _rule.currencyItem.icon : null;
            moneyIcon.enabled = moneyIcon.sprite != null;
            moneyIcon.preserveAspect = true;
        }

        if (moneyNumber != null) moneyNumber.AnimateTo(money);

        bool hasCoupon = HasCoupon(inv);
        float mul = Mathf.Clamp(_rule.discountMultiplier, 0.01f, 1f);

        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].SetDiscountState(hasCoupon, mul);
            _slots[i].SetSelected(_slots[i] == _selected);
        }

        RefreshBuyButtonState();

        if (sellListUI != null)
            sellListUI.Refresh(_rule);

        RefreshSellButtonState();
    }

    private void RefreshBuyButtonState()
    {
        if (buyButton == null) return;
        if (_rule == null || _selected == null || !_selected.IsSelectable) { buyButton.interactable = false; return; }

        int money = GetMoneyAmount(InventoryManager.Instance);
        buyButton.interactable = money >= _selected.CurrentPrice;
    }

    private void OnClickBuy()
    {
        if (_rule == null || _rule.currencyItem == null || _selected == null || !_selected.IsSelectable) return;

        var inv = InventoryManager.Instance;
        if (inv == null) return;

        int price = _selected.CurrentPrice;
        if (GetMoneyAmount(inv) < price) { RefreshBuyButtonState(); return; }

        if (inv.RemoveItem(_rule.currencyItem.itemID, price) < price) return;

        if (!_selected.ApplyPurchase(inv))
        {
            inv.AddItem(_rule.currencyItem.itemID, price); // 回滚
            return;
        }

        RefreshAll();
    }

    private int GetMoneyAmount(InventoryManager inv)
    {
        if (inv == null || _rule == null || _rule.currencyItem == null) return 0;
        return inv.GetTotalAmount(_rule.currencyItem.itemID);
    }

    private bool HasCoupon(InventoryManager inv)
    {
        if (inv == null || _rule == null || _rule.couponItem == null) return false;
        return inv.GetTotalAmount(_rule.couponItem.itemID) > 0;
    }
}
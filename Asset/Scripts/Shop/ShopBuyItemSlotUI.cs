using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopBuyItemSlotUI : MonoBehaviour
{
    [Header("【UI】基础显示")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text stockText;

    [Header("【UI】折扣标签")]
    [SerializeField] private Image discountTag;

    [Header("【交互】选择按钮与背景")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Image background;

    [Header("【颜色】普通/选中/售罄")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
    [SerializeField] private Color soldOutColor = new Color(0.45f, 0.45f, 0.45f, 1f);

    private ShopBuyEntry _entry;
    private ShopPanelUI _owner;
    private bool _hasCoupon;
    private float _discountMultiplier = 1f;
    private bool _selected;

    public bool IsSelectable => _entry != null && (_entry.stock != 0) && _entry.item != null;
    public int CurrentPrice => CalcPrice(_entry, _hasCoupon, _discountMultiplier);

    public void Bind(ShopBuyEntry entry, ShopPanelUI owner)
    {
        _entry = entry;
        _owner = owner;

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() =>
            {
                if (IsSelectable) _owner?.OnSelectSlot(this);
            });
        }

        RefreshVisual();
    }

    public void SetDiscountState(bool hasCoupon, float discountMultiplier)
    {
        _hasCoupon = hasCoupon;
        _discountMultiplier = Mathf.Clamp(discountMultiplier, 0.01f, 1f);
        RefreshVisual();
    }

    public void SetSelected(bool selected)
    {
        _selected = selected;
        RefreshVisual();
    }

    public bool ApplyPurchase(InventoryManager inv)
    {
        if (inv == null || _entry == null || _entry.item == null) return false;
        if (_entry.stock == 0) return false;

        inv.AddItem(_entry.item.itemID, Mathf.Max(1, _entry.giveAmount));

        if (_entry.stock > 0) _entry.stock -= 1;
        RefreshVisual();
        return true;
    }

    private void RefreshVisual()
    {
        if (_entry == null || _entry.item == null)
        {
            if (gameObject.activeSelf) gameObject.SetActive(false);
            return;
        }

        if (!gameObject.activeSelf) gameObject.SetActive(true);

        if (icon != null)
        {
            icon.sprite = _entry.item.icon;
            icon.enabled = _entry.item.icon != null;
            icon.preserveAspect = true;
        }

        if (nameText != null) nameText.text = _entry.item.itemName;
        if (descText != null) descText.text = _entry.item.description;
        if (priceText != null) priceText.text = CurrentPrice.ToString();

        if (stockText != null)
            stockText.text = _entry.stock < 0 ? "∞" : _entry.stock.ToString();

        if (discountTag != null)
            discountTag.gameObject.SetActive(_hasCoupon);

        bool soldOut = _entry.stock == 0;
        if (selectButton != null) selectButton.interactable = !soldOut;

        if (background != null)
        {
            if (soldOut) background.color = soldOutColor;
            else background.color = _selected ? selectedColor : normalColor;
        }
    }

    private static int CalcPrice(ShopBuyEntry e, bool hasCoupon, float mul)
    {
        if (e == null) return int.MaxValue;
        int basePrice = Mathf.Max(1, e.priceCurrency);

        if (!hasCoupon) return basePrice;

        int discounted = Mathf.CeilToInt(basePrice * Mathf.Clamp(mul, 0.01f, 1f));
        return Mathf.Max(1, discounted);
    }
}
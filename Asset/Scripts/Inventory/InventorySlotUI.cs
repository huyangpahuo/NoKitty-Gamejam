using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Purely visual representation of one inventory slot. Owns no inventory
/// logic and never talks to InventoryManager to mutate data — it only
/// displays whatever it is told to by InventoryUI. Hover-to-show-info
/// forwards to ItemUIManager (another display-layer class).
///
/// Also acts as a drag SOURCE only: it lets the player pick up one unit of
/// its item to drop onto a crafting slot. Dragging never mutates inventory
/// data — that's why there's no "return to slot" logic here: the slot's
/// own display never changes during a drag, so there's nothing to revert.
///
/// Additionally supports click-based selection (used by the shop's sell
/// list). This is purely visual/event-based — it does not mutate
/// inventory data and does not interfere with drag behaviour.
/// </summary>
public class InventorySlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
    IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text countText;

    [Header("【选中高亮】(用于可点击选中的场景，如商店出售列表)")]
    [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.4f);

    private Color _normalColor = Color.white;
    private ItemData _currentItem;

    public ItemData CurrentItem => _currentItem;

    /// <summary>Raised when this slot is clicked while it holds an item.</summary>
    public event Action<InventorySlotUI> Clicked;

    public void Awake()
    {
        if (icon != null)
        {
            icon.preserveAspect = true;
        }

        if (background != null)
        {
            _normalColor = background.color;
        }
    }

    public void SetData(ItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0)
        {
            Clear();
            return;
        }

        _currentItem = itemData;

        icon.enabled = true;
        icon.sprite = itemData.icon;
        countText.text = amount > 1 ? amount.ToString() : string.Empty;
    }

    public void Clear()
    {
        _currentItem = null;

        icon.enabled = false;
        icon.sprite = null;
        countText.text = string.Empty;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_currentItem == null) return;
        if (ItemUIManager.Instance == null) return;

        ItemUIManager.Instance.ShowItemInfo(_currentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemUIManager.Instance == null) return;

        ItemUIManager.Instance.NoSoundCloseItemInfo();
    }

    // ----- Click selection (used by shop sell list; harmless elsewhere) -----

    /// <summary>Sets the highlight state for click-based selection. Does not affect drag.</summary>
    public void SetSelected(bool selected)
    {
        if (background != null)
            background.color = selected ? selectedColor : _normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_currentItem == null) return;

        Clicked?.Invoke(this);
    }

    // ----- Drag source: pick up 1 unit to drop onto a crafting slot -----

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_currentItem == null) return;

        DragContext.Begin(DragContext.SourceKind.InventorySlot, _currentItem.itemID);

        if (DragIconController.Instance != null)
            DragIconController.Instance.Begin(_currentItem.icon);
    }


    public void OnDrag(PointerEventData eventData)
    {
        // The icon follows the mouse on its own via DragIconController.Update.
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Whether a crafting slot accepted the drop or it landed on empty
        // space, this slot's own display never changed — inventory data
        // is untouched by dragging, so there's nothing to revert here.
        if (DragIconController.Instance != null)
            DragIconController.Instance.Hide();

        DragContext.End();
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Purely visual representation of one crafting ingredient slot (A or B).
/// Owns no crafting logic itself — reads CraftingSlotData for display, and
/// forwards drag/drop gestures to CraftingManager, which decides whether
/// to accept them.
/// </summary>
public class CraftingSlotUI : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text countText;

    [Tooltip("0 = ingredient slot A, 1 = ingredient slot B")]
    [SerializeField] private int slotIndex;

    private void Awake()
    {
        if (icon != null)
            icon.preserveAspect = true;
    }

    private DragContext.SourceKind MySourceKind =>
        slotIndex == 0 ? DragContext.SourceKind.CraftingSlotA : DragContext.SourceKind.CraftingSlotB;

    private CraftingSlotData CurrentData
    {
        get
        {
            var cm = CraftingManager.Instance;
            if (cm == null) return null;
            return slotIndex == 0 ? cm.SlotA : cm.SlotB;
        }
    }

    public void Refresh(CraftingSlotData data)
    {
        if (icon == null || countText == null) return;

        if (data == null || data.IsEmpty)
        {
            icon.enabled = false;
            icon.sprite = null;
            countText.text = string.Empty;
            return;
        }

        ItemData itemData = ItemDatabase.GetItem(data.itemId);
        if (itemData == null)
        {
            icon.enabled = false;
            icon.sprite = null;
            countText.text = string.Empty;
            return;
        }

        icon.enabled = true;
        icon.sprite = itemData.icon;
        icon.preserveAspect = true;
        countText.text = data.amount > 1 ? data.amount.ToString() : string.Empty;
    }

    // Right click: quick unstage
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;

        var cm = CraftingManager.Instance;
        if (cm == null) return;

        cm.ClearSlot(slotIndex);
    }

    // ----- Accepting a drop from an inventory slot -----

    public void OnDrop(PointerEventData eventData)
    {
        if (DragContext.Source != DragContext.SourceKind.InventorySlot) return;

        var cm = CraftingManager.Instance;
        if (cm == null) return;

        if (cm.TryStageItem(slotIndex, DragContext.ItemId))
            DragContext.MarkConsumed();
    }

    // ----- Dragging a staged item back out to unstage it -----

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        var cm = CraftingManager.Instance;
        if (cm == null) return;

        CraftingSlotData data = CurrentData;
        if (data == null || data.IsEmpty) return;

        string draggedItemId = data.itemId;
        ItemData itemData = ItemDatabase.GetItem(draggedItemId);

        DragContext.Begin(MySourceKind, draggedItemId);

        if (DragIconController.Instance != null)
            DragIconController.Instance.Begin(itemData != null ? itemData.icon : null);

        // 你要的行为：拿起时立刻从槽位消失
        cm.ClearSlot(slotIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Drag icon follows mouse in DragIconController.Update()
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (DragIconController.Instance != null)
            DragIconController.Instance.Hide();

        var cm = CraftingManager.Instance;

        // 若没放到有效目标，放回原槽，避免“拖到空白处直接丢失”
        // if (!DragContext.WasConsumed && cm != null && !string.IsNullOrEmpty(DragContext.ItemId))
        // {
        //     cm.TryStageItem(slotIndex, DragContext.ItemId);
        // }

        DragContext.End();
    }
}
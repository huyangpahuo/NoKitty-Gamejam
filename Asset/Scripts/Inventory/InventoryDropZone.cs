using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Put this on the inventory panel's background / scroll view content
/// area. Its only job is to catch drags coming FROM a crafting ingredient
/// slot and unstage them, so dropping a reserved item back onto the
/// inventory releases the reservation. Since the reservation is UI-only,
/// "returning" an item just means clearing the crafting slot — nothing
/// needs to be added back to inventory data, it was never removed.
///
/// Drops originating from the inventory itself are ignored; this system
/// doesn't support inventory-to-inventory rearranging.
/// </summary>
public class InventoryDropZone : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (!DragContext.IsDragging) return;

        var cm = CraftingManager.Instance;
        if (cm == null) return;

        switch (DragContext.Source)
        {
            case DragContext.SourceKind.CraftingSlotA:
                cm.ClearSlot(0);
                DragContext.MarkConsumed();
                break;
            case DragContext.SourceKind.CraftingSlotB:
                cm.ClearSlot(1);
                DragContext.MarkConsumed();
                break;
        }
    }
}
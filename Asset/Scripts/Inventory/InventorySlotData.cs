/// <summary>
/// Lightweight runtime representation of one inventory slot.
/// Only ever holds an item id and an amount — never a GameObject,
/// ItemData reference, or UI reference. Owned exclusively by InventoryManager.
/// </summary>
[System.Serializable]
public class InventorySlotData
{
    public string itemId;
    public int amount;

    public bool IsEmpty => string.IsNullOrEmpty(itemId) || amount <= 0;

    public InventorySlotData()
    {
        itemId = null;
        amount = 0;
    }

    public InventorySlotData(string itemId, int amount)
    {
        this.itemId = itemId;
        this.amount = amount;
    }

    public void Clear()
    {
        itemId = null;
        amount = 0;
    }
}
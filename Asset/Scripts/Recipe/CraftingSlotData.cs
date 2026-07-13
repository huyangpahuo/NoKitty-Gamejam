/// <summary>
/// Runtime representation of one crafting ingredient slot. Structurally
/// identical to InventorySlotData, but deliberately kept separate: this
/// data is a UI-only reservation owned by CraftingManager and is never
/// saved and never part of the real inventory until Craft() commits it.
/// </summary>
[System.Serializable]
public class CraftingSlotData
{
    public string itemId;
    public int amount;

    public bool IsEmpty => string.IsNullOrEmpty(itemId) || amount <= 0;

    public void Clear()
    {
        itemId = null;
        amount = 0;
    }
}
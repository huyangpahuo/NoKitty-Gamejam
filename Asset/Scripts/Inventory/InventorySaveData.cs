/// <summary>
/// Pure persistence model for one inventory entry — itemId and amount only.
/// Deliberately kept separate from InventorySlotData so the save file
/// format can evolve independently of the runtime inventory representation.
/// Referenced by SettingsData.inventory and written/read by SettingsManager.
/// </summary>
[System.Serializable]
public class InventorySaveData
{
    public string itemId;
    public int amount;

    public InventorySaveData() { }

    public InventorySaveData(string itemId, int amount)
    {
        this.itemId = itemId;
        this.amount = amount;
    }
}
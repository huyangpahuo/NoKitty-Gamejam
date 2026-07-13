using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Sole owner and sole mutator of inventory data. No other class is allowed
/// to modify the slot list directly. InventoryUI and everything else only
/// reads through the public API and reacts to the change events.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    private static InventoryManager _instance;

    /// <summary>
    /// Lazily resolves the singleton regardless of script execution order.
    /// If another component's OnEnable/Start asks for Instance before this
    /// object's own Awake has run, this finds (or creates) it on demand
    /// instead of returning null.
    /// </summary>
    public static InventoryManager Instance
    {
        get
        {
            if (_instance == null)
            {
                if (_instance == null)
                    _instance = FindObjectOfType<InventoryManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("InventoryManager");
                    _instance = go.AddComponent<InventoryManager>();
                }
            }

            // FindObjectOfType can return an instance whose own Awake()
            // hasn't run yet (Unity doesn't guarantee Awake order across
            // unrelated GameObjects). Force-initialize here so callers
            // never see an instance with a null _slots list.
            if (_instance != null)
                _instance.EnsureInitialized();

            return _instance;
        }
    }

    [SerializeField] private int initialSlotCount = 25;

    private List<InventorySlotData> _slots;
    private bool _isLoading;
    private bool _isInitialized;

    public IReadOnlyList<InventorySlotData> Slots
    {
        get
        {
            EnsureInitialized();
            return _slots;
        }
    }

    public int SlotCount
    {
        get
        {
            EnsureInitialized();
            return _slots.Count;
        }
    }

    /// <summary>Fired when slot contents change (icons/counts need redrawing).</summary>
    public event Action OnInventoryChanged;

    /// <summary>Fired when the number of slots changes (UI must spawn/remove slot UIs).</summary>
    public event Action<int> OnCapacityChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // DontDestroyOnLoad only works on root GameObjects. If this object
        // is ever accidentally parented (e.g. nested inside another prefab
        // during scene setup), force it back to root first so persistence
        // doesn't silently fail.
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        EnsureInitialized();
    }

    private void OnDestroy()
    {
        // Release the static reference if this is the active instance being
        // destroyed, so a stale/destroyed object is never handed back to
        // future callers of Instance, and so the next Instance access knows
        // to find-or-create cleanly instead of pointing at a dead object.
        if (_instance == this)
            _instance = null;
    }

    /// <summary>
    /// Idempotent data initialization, decoupled from Unity's Awake() timing.
    /// Safe to call multiple times and from any code path (Instance getter,
    /// Awake, or directly) — guarantees _slots is populated before any
    /// caller touches it, regardless of script execution order.
    /// </summary>
    private void EnsureInitialized()
    {
        if (_isInitialized) return;
        _isInitialized = true;

        LoadFromDisk();
    }

    private void InitializeEmptySlots(int count)
    {
        _slots = new List<InventorySlotData>(count);
        for (int i = 0; i < count; i++)
            _slots.Add(new InventorySlotData());
    }

    /// <summary>
    /// Grows the inventory by adding empty slots. Triggers OnCapacityChanged
    /// so the UI can instantiate matching slot UI objects.
    /// </summary>
    public void ExpandCapacity(int additionalSlots)
    {
        EnsureInitialized();
        if (additionalSlots <= 0) return;

        for (int i = 0; i < additionalSlots; i++)
            _slots.Add(new InventorySlotData());

        OnCapacityChanged?.Invoke(_slots.Count);
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// Adds an item, stacking into existing matching slots first, then
    /// filling empty slots. Returns the amount that did NOT fit
    /// (0 means everything was added successfully).
    /// </summary>
    public int AddItem(string itemId, int amount)
    {
        EnsureInitialized();

        if (string.IsNullOrEmpty(itemId) || amount <= 0)
            return amount;

        ItemData itemData = ItemDatabase.GetItem(itemId);
        if (itemData == null)
        {
            Debug.LogWarning($"[InventoryManager] AddItem failed: unknown itemId '{itemId}'");
            return amount;
        }

        int remaining = amount;

        // 1. Top up existing stacks of the same item.
        foreach (InventorySlotData slot in _slots)
        {
            if (remaining <= 0) break;
            if (slot.itemId != itemId) continue;

            int space = itemData.maxStack - slot.amount;
            if (space <= 0) continue;

            int toAdd = Mathf.Min(space, remaining);
            slot.amount += toAdd;
            remaining -= toAdd;
        }

        // 2. Place any leftover into empty slots, creating new stacks.
        foreach (InventorySlotData slot in _slots)
        {
            if (remaining <= 0) break;
            if (!slot.IsEmpty) continue;

            int toAdd = Mathf.Min(itemData.maxStack, remaining);
            slot.itemId = itemId;
            slot.amount = toAdd;
            remaining -= toAdd;
        }

        if (remaining != amount)
        {
            OnInventoryChanged?.Invoke();
            if (!_isLoading) SaveToDisk();
        }

        return remaining;
    }

    /// <summary>
    /// Removes up to 'amount' of itemId across all slots.
    /// Returns the amount actually removed.
    /// </summary>
    public int RemoveItem(string itemId, int amount)
    {
        EnsureInitialized();

        if (string.IsNullOrEmpty(itemId) || amount <= 0)
            return 0;

        int remaining = amount;

        foreach (InventorySlotData slot in _slots)
        {
            if (remaining <= 0) break;
            if (slot.itemId != itemId) continue;

            int toRemove = Mathf.Min(slot.amount, remaining);
            slot.amount -= toRemove;
            remaining -= toRemove;

            if (slot.amount <= 0)
                slot.Clear();
        }

        int removed = amount - remaining;
        if (removed > 0)
        {
            OnInventoryChanged?.Invoke();
            if (!_isLoading) SaveToDisk();
        }

        return removed;
    }

    public int GetTotalAmount(string itemId)
    {
        EnsureInitialized();
        return _slots.Where(s => s.itemId == itemId).Sum(s => s.amount);
    }

    // ===================== Persistence =====================

    /// <summary>Converts current non-empty slots into the save model.</summary>
    public List<InventorySaveData> ToSaveData()
    {
        EnsureInitialized();
        return _slots
            .Where(s => !s.IsEmpty)
            .Select(s => new InventorySaveData(s.itemId, s.amount))
            .ToList();
    }

    /// <summary>
    /// Rebuilds inventory data from a loaded save list and notifies the UI.
    /// Grows capacity automatically if the save data has more entries than
    /// the current slot count.
    /// </summary>
    public void LoadFromSaveData(List<InventorySaveData> saveData)
    {
        _isLoading = true;

        InitializeEmptySlots(initialSlotCount);

        if (saveData != null)
        {
            int index = 0;
            foreach (InventorySaveData entry in saveData)
            {
                if (index >= _slots.Count)
                    _slots.Add(new InventorySlotData());

                _slots[index].itemId = entry.itemId;
                _slots[index].amount = entry.amount;
                index++;
            }
        }

        _isLoading = false;

        OnCapacityChanged?.Invoke(_slots.Count);
        OnInventoryChanged?.Invoke();
    }

    /// <summary>Persists current inventory into the shared SettingsData/settings.json.</summary>
    public void SaveToDisk()
    {
        SettingsData data = SettingsManager.Load() ?? new SettingsData();
        data.inventory = ToSaveData();
        SettingsManager.Save(data);
    }

    /// <summary>Loads inventory from settings.json (if present) and refreshes the UI.</summary>
    public void LoadFromDisk()
    {
        SettingsData data = SettingsManager.Load();
        LoadFromSaveData(data?.inventory);
    }
}
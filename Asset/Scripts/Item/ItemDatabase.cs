using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Static registry of every ItemData asset in the project.
/// Loads once at game startup (before any scene logic runs) and is
/// queried by id everywhere else. Place all ItemData assets under
/// Assets/Resources/Items/ for this to find them.
/// </summary>
public static class ItemDatabase
{
    private static Dictionary<string, ItemData> _items;
    private static bool _isLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        Initialize();
    }

    public static void Initialize()
    {
        if (_isLoaded) return;

        _items = new Dictionary<string, ItemData>();

        ItemData[] allItems = Resources.LoadAll<ItemData>("Items");

        foreach (ItemData item in allItems)
        {
            if (string.IsNullOrEmpty(item.itemID))
            {
                Debug.LogWarning($"[ItemDatabase] '{item.name}' has an empty itemID and was skipped.");
                continue;
            }

            if (_items.ContainsKey(item.itemID))
            {
                Debug.LogError($"[ItemDatabase] Duplicate itemID '{item.itemID}' on '{item.name}'. Skipped.");
                continue;
            }

            _items.Add(item.itemID, item);
        }

        _isLoaded = true;
        Debug.Log($"[ItemDatabase] Initialized with {_items.Count} items.");
    }

    public static ItemData GetItem(string itemId)
    {
        if (!_isLoaded) Initialize();

        if (string.IsNullOrEmpty(itemId)) return null;

        return _items.TryGetValue(itemId, out ItemData item) ? item : null;
    }

    public static bool TryGetItem(string itemId, out ItemData item)
    {
        if (!_isLoaded) Initialize();

        return _items.TryGetValue(itemId, out item);
    }

    public static bool Contains(string itemId)
    {
        if (!_isLoaded) Initialize();

        return !string.IsNullOrEmpty(itemId) && _items.ContainsKey(itemId);
    }
}
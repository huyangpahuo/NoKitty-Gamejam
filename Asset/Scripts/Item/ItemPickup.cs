using UnityEngine;

/// <summary>
/// A world-space item. References ItemData only — never touches inventory
/// data structures directly, just asks InventoryManager to add the item.
///
/// Each scene-placed instance gets a stable, unique pickupId baked in via
/// OnValidate (editor-time only). That id is checked against the persisted
/// "collected" list on Start so an already-picked-up item never re-appears
/// after a scene reload or game restart.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    [Tooltip("Auto-generated unique id for this specific world instance. Do not edit manually.")]
    [SerializeField, HideInInspector] private string pickupId;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Bake a stable GUID the first time this object is configured in
        // the editor. Runtime-instantiated copies (e.g. spawned loot) will
        // have an empty id here by design — see Start().
        if (string.IsNullOrEmpty(pickupId))
            pickupId = System.Guid.NewGuid().ToString();
    }
#endif

    private void Start()
    {
        // Scene-placed instance already collected in a previous session.
        if (!string.IsNullOrEmpty(pickupId) && SettingsManager.IsPickupCollected(pickupId))
        {
            Destroy(gameObject);
            return;
        }

        // Runtime-spawned pickups (no baked id) get a unique id per-instance
        // so they can still be tracked if they need to survive a reload
        // (e.g. dropped-on-death loot you want to persist).
        if (string.IsNullOrEmpty(pickupId))
            pickupId = System.Guid.NewGuid().ToString();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (itemData == null)
        {
            Debug.LogWarning($"[ItemPickup] '{name}' has no ItemData assigned.");
            return;
        }

        int leftover = InventoryManager.Instance.AddItem(itemData.itemID, amount);

        if (leftover <= 0)
        {
            // Everything fit — record this instance as collected so it
            // never respawns, then remove it from the world.
            SettingsManager.MarkPickupCollected(pickupId);
            Destroy(gameObject);
        }
        else
        {
            // Inventory full: only part of the stack was picked up,
            // keep the remainder in the world (not marked collected).
            amount = leftover;
        }
    }
}
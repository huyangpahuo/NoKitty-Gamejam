using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the Scroll View / Grid Layout Group of slot UIs. Never modifies
/// inventory data — only instantiates InventorySlotUI objects and refreshes
/// what they display, driven entirely by InventoryManager's events.
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Grid Layout Group container (Scroll View content)")]
    [SerializeField] private Transform slotParent;

    [SerializeField] private InventorySlotUI slotPrefab;
    [SerializeField] private int initialSlotCount = 25;

    [Header("Panel Toggle")]
    [Tooltip("The panel GameObject to show/hide. Keep this script on a separate, always-enabled object so the toggle key keeps working while the panel is closed.")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private KeyCode toggleKey = KeyCode.B;

    [Header("Crafting Panel (opens/closes with inventory)")]
    [SerializeField] private GameObject craftingPanelRoot;

    private readonly List<InventorySlotUI> _slotUIs = new();

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Awake()
    {
        for (int i = 0; i < initialSlotCount; i++)
            CreateSlotUI();
    }

    private void OnEnable()
    {
        var inv = InventoryManager.Instance;
        if (inv != null)
        {
            inv.OnInventoryChanged += RefreshDisplay;
            inv.OnCapacityChanged += HandleCapacityChanged;
        }

        var craft = CraftingManager.Instance;
        if (craft != null)
        {
            craft.OnCraftingChanged += RefreshDisplay; // 你之前加过这个的话保留
        }

        RefreshDisplay();
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance == null) return;

        InventoryManager.Instance.OnInventoryChanged -= RefreshDisplay;
        InventoryManager.Instance.OnCapacityChanged -= HandleCapacityChanged;

        if (CraftingManager.Instance != null)
            CraftingManager.Instance.OnCraftingChanged -= RefreshDisplay;
    }

    private InventorySlotUI CreateSlotUI()
    {
        InventorySlotUI slot = Instantiate(slotPrefab, slotParent);
        _slotUIs.Add(slot);
        return slot;
    }

    private void HandleCapacityChanged(int newCapacity)
    {
        while (_slotUIs.Count < newCapacity)
            CreateSlotUI();

        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return;

        IReadOnlyList<InventorySlotData> data = inv.Slots;

        if (data == null) return;

        for (int i = 0; i < _slotUIs.Count; i++)
        {
            if (i >= data.Count)
            {
                _slotUIs[i].Clear();
                continue;
            }

            InventorySlotData slotData = data[i];

            if (slotData.IsEmpty)
            {
                _slotUIs[i].Clear();
                continue;
            }

            ItemData itemData = ItemDatabase.GetItem(slotData.itemId);
            if (itemData == null)
            {
                // Save data references an itemId that no longer exists in
                // ItemDatabase (deleted/renamed asset). Don't crash the UI —
                // just show the slot as empty and flag it for investigation.
                Debug.LogWarning($"[InventoryUI] No ItemData found for itemId '{slotData.itemId}' in slot {i}.");
                _slotUIs[i].Clear();
                continue;
            }

            _slotUIs[i].SetData(itemData, slotData.amount);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    /// <summary>Opens the inventory panel. Safe to call from a UI Button OnClick event.</summary>
    public void Open()
    {
        if (panelRoot == null) return;

        PlayUISound();

        panelRoot.SetActive(true);

        if (craftingPanelRoot != null)
            craftingPanelRoot.SetActive(true);

        RefreshDisplay(); // catch up on any changes that happened while closed
    }

    /// <summary>Closes the inventory panel. Safe to call from a UI Button OnClick event.</summary>
    public void Close()
    {
        if (panelRoot == null) return;

        PlayUISound();

        // Auto-return staged items (staging is UI-only => just clear slots)
        if (CraftingManager.Instance != null)
        {
            CraftingManager.Instance.ClearSlot(0);
            CraftingManager.Instance.ClearSlot(1);
        }

        if (craftingPanelRoot != null)
            craftingPanelRoot.SetActive(false);

        panelRoot.SetActive(false);
    }

    public void Toggle()
    {
        if (IsOpen) Close();
        else Open();
    }

    public void PlayUISound()
    {
        AudioManager.Instance.PlaySFX("ClickSoundsClip");
    }
}
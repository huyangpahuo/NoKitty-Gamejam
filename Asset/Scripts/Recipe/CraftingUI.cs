using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Display-only crafting panel. Reflects CraftingManager's staged slots
/// and matched recipe. The Craft button is the only thing that triggers
/// an actual inventory mutation, via CraftingManager.Craft() — everything
/// else here is pure display driven by CraftingManager's events.
/// </summary>
public class CraftingUI : MonoBehaviour
{
    [Header("Ingredient slots")]
    [SerializeField] private CraftingSlotUI slotAUI;
    [SerializeField] private CraftingSlotUI slotBUI;

    [Header("Result display")]
    [SerializeField] private Image resultIcon;
    [SerializeField] private TMP_Text resultCountText;
    [SerializeField] private Button craftButton;

    private void Awake()
    {
        if (resultIcon != null)
        {
            resultIcon.preserveAspect = true;
        }
    }
    private void OnEnable()
    {
        var cm = CraftingManager.Instance;
        if (cm != null)
            cm.OnCraftingChanged += Refresh;

        if (craftButton != null)
            craftButton.onClick.AddListener(HandleCraftPressed);

        Refresh();
    }

    private void OnDisable()
    {
        var cm = CraftingManager.Instance;
        if (cm != null)
            cm.OnCraftingChanged -= Refresh;

        if (craftButton != null)
            craftButton.onClick.RemoveListener(HandleCraftPressed);
    }

    private void HandleCraftPressed()
    {
        var cm = CraftingManager.Instance;
        if (cm != null)
            cm.Craft();
    }

    private void Refresh()
    {
        var cm = CraftingManager.Instance;
        if (cm == null)
        {
            if (slotAUI != null) slotAUI.Refresh(null);
            if (slotBUI != null) slotBUI.Refresh(null);

            if (craftButton != null) craftButton.interactable = false;
            if (resultIcon != null) { resultIcon.enabled = false; resultIcon.sprite = null; }
            if (resultCountText != null) resultCountText.text = string.Empty;
            return;
        }

        if (slotAUI != null) slotAUI.Refresh(cm.SlotA);
        if (slotBUI != null) slotBUI.Refresh(cm.SlotB);

        RecipeData recipe = cm.MatchedRecipe;


        if (recipe == null)
        {
            // No recipe matches the ingredients placed — empty result, disabled button.
            resultIcon.enabled = false;
            resultIcon.sprite = null;
            resultCountText.text = string.Empty;
            craftButton.interactable = false;
            return;
        }

        ItemData resultItem = ItemDatabase.GetItem(recipe.resultItem.itemID);
        resultIcon.enabled = resultItem != null;
        resultIcon.sprite = resultItem != null ? resultItem.icon : null;
        resultCountText.text = recipe.resultAmount > 1 ? recipe.resultAmount.ToString() : string.Empty;

        // Recipe exists: always show the result. Only sufficiency gates the button.
        craftButton.interactable = cm.CanCraft;
    }
}
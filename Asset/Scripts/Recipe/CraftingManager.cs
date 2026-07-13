using System;
using UnityEngine;

/// <summary>
/// Sole owner and sole mutator of crafting state.
/// </summary>
public class CraftingManager : MonoBehaviour
{
    private static CraftingManager _instance;

    public static CraftingManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<CraftingManager>();
            return _instance;
        }
    }

    private readonly CraftingSlotData _slotA = new CraftingSlotData();
    private readonly CraftingSlotData _slotB = new CraftingSlotData();

    public CraftingSlotData SlotA => _slotA;
    public CraftingSlotData SlotB => _slotB;

    public RecipeData MatchedRecipe { get; private set; }
    public bool CanCraft { get; private set; }

    public event Action OnCraftingChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    private void OnDestroy()
    {
        if (_instance == this) _instance = null;
    }

    public bool TryStageItem(int slotIndex, string itemId)
    {
        if (string.IsNullOrEmpty(itemId)) return false;

        var inv = InventoryManager.Instance;
        if (inv == null) return false;

        CraftingSlotData slot = slotIndex == 0 ? _slotA : _slotB;

        if (!slot.IsEmpty && slot.itemId != itemId)
            return false;

        int owned = inv.GetTotalAmount(itemId);
        int alreadyStaged = StagedAmountOf(itemId);
        if (alreadyStaged >= owned)
            return false;

        if (slot.IsEmpty) slot.itemId = itemId;
        slot.amount += 1;

        RecomputeMatch();
        return true;
    }

    public void ClearSlot(int slotIndex)
    {
        CraftingSlotData slot = slotIndex == 0 ? _slotA : _slotB;
        if (slot.IsEmpty) return;

        slot.Clear();
        RecomputeMatch();
    }

    private int StagedAmountOf(string itemId)
    {
        int total = 0;
        if (_slotA.itemId == itemId) total += _slotA.amount;
        if (_slotB.itemId == itemId) total += _slotB.amount;
        return total;
    }

    private void RecomputeMatch()
    {
        MatchedRecipe = RecipeDatabase.FindMatchByItems(_slotA.itemId, _slotB.itemId);
        CanCraft = MatchedRecipe != null && HasSufficientAmounts(MatchedRecipe);
        OnCraftingChanged?.Invoke();
    }

    private bool HasSufficientAmounts(RecipeData recipe)
    {
        if (recipe == null || recipe.ingredientA == null || recipe.ingredientA.item == null)
            return false;

        string recipeAId = recipe.ingredientA.item.itemID;
        string recipeBId = (recipe.ingredientB != null && recipe.ingredientB.item != null)
            ? recipe.ingredientB.item.itemID
            : null;

        int requiredA = Mathf.Max(1, recipe.ingredientA.amount);
        int requiredB = !string.IsNullOrEmpty(recipeBId)
            ? Mathf.Max(1, recipe.ingredientB.amount)
            : 0;

        // 单材料配方：只看匹配槽位是否够数量，另一槽必须空（匹配阶段已保证）
        if (string.IsNullOrEmpty(recipeBId))
        {
            if (_slotA.itemId == recipeAId) return _slotA.amount >= requiredA;
            if (_slotB.itemId == recipeAId) return _slotB.amount >= requiredA;
            return false;
        }

        // 双材料配方
        bool aIsRecipeA = _slotA.itemId == recipeAId;
        int haveForA = aIsRecipeA ? _slotA.amount : _slotB.amount;
        int haveForB = aIsRecipeA ? _slotB.amount : _slotA.amount;

        if (haveForA < requiredA) return false;
        if (haveForB < requiredB) return false;
        return true;
    }

    public void Craft()
    {
        if (!CanCraft || MatchedRecipe == null) return;

        var inv = InventoryManager.Instance;
        if (inv == null) return;

        if (MatchedRecipe.ingredientA == null || MatchedRecipe.ingredientA.item == null)
            return;
        if (MatchedRecipe.resultItem == null || string.IsNullOrEmpty(MatchedRecipe.resultItem.itemID))
            return;

        string recipeAId = MatchedRecipe.ingredientA.item.itemID;
        string recipeBId = (MatchedRecipe.ingredientB != null && MatchedRecipe.ingredientB.item != null)
            ? MatchedRecipe.ingredientB.item.itemID
            : null;

        int needA = Mathf.Max(1, MatchedRecipe.ingredientA.amount);
        int needB = !string.IsNullOrEmpty(recipeBId)
            ? Mathf.Max(1, MatchedRecipe.ingredientB.amount)
            : 0;

        if (string.IsNullOrEmpty(recipeBId))
        {
            // 单材料：从包含A材料的那个槽位扣
            string fromId = _slotA.itemId == recipeAId ? _slotA.itemId : _slotB.itemId;
            if (string.IsNullOrEmpty(fromId)) return;

            inv.RemoveItem(fromId, needA);
        }
        else
        {
            bool aIsRecipeA = _slotA.itemId == recipeAId;

            string idA = aIsRecipeA ? _slotA.itemId : _slotB.itemId;
            string idB = aIsRecipeA ? _slotB.itemId : _slotA.itemId;

            inv.RemoveItem(idA, needA);
            inv.RemoveItem(idB, needB);
        }

        inv.AddItem(MatchedRecipe.resultItem.itemID, Mathf.Max(1, MatchedRecipe.resultAmount));

        _slotA.Clear();
        _slotB.Clear();
        RecomputeMatch();
    }
}
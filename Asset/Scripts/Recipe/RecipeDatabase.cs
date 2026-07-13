using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static registry of every RecipeData asset in the project. Loads once at
/// startup, same pattern as ItemDatabase. Queried by the item types placed
/// in the two crafting slots.
/// </summary>
public static class RecipeDatabase
{
    private static List<RecipeData> _recipes;
    private static bool _isLoaded;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoInitialize()
    {
        Initialize();
    }

    public static void Initialize()
    {
        if (_isLoaded) return;

        _recipes = new List<RecipeData>(Resources.LoadAll<RecipeData>("Recipes"));
        _isLoaded = true;

        Debug.Log($"[RecipeDatabase] Initialized with {_recipes.Count} recipes.");
    }

    /// <summary>
    /// Finds a recipe whose ingredient item TYPES match the two staged
    /// slots, ignoring quantity and slot order (Stone+Wood == Wood+Stone).
    /// Quantity sufficiency is checked separately by CraftingManager, so
    /// the UI can still preview a result even when not enough materials
    /// are staged yet.
    /// </summary>
    public static RecipeData FindMatchByItems(string itemIdA, string itemIdB)
    {
        if (!_isLoaded) Initialize();

        foreach (RecipeData recipe in _recipes)
        {
            if (ItemPairMatches(recipe, itemIdA, itemIdB))
                return recipe;
        }

        return null;
    }

    private static bool ItemPairMatches(RecipeData recipe, string idA, string idB)
    {
        string rA = recipe.ingredientA != null && recipe.ingredientA.item != null
            ? recipe.ingredientA.item.itemID : null;
        string rB = recipe.ingredientB != null && recipe.ingredientB.item != null
            ? recipe.ingredientB.item.itemID : null;

        bool recipeHasB = !string.IsNullOrEmpty(rB);

        if (string.IsNullOrEmpty(rA)) return false;

        if (!recipeHasB)
        {
            return (idA == rA && string.IsNullOrEmpty(idB))
                || (idB == rA && string.IsNullOrEmpty(idA));
        }

        if (string.IsNullOrEmpty(idA) || string.IsNullOrEmpty(idB))
            return false;

        return (idA == rA && idB == rB) || (idA == rB && idB == rA);
    }
}
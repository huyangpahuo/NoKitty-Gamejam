using UnityEngine;

/// <summary>
/// A craftable recipe: up to two ingredient requirements and one result.
/// Place all RecipeData assets under Assets/Resources/Recipes/ so
/// RecipeDatabase can find them.
///
/// Leave ingredientB.itemID empty for a single-ingredient recipe.
/// </summary>
[CreateAssetMenu(fileName = "New Recipe", menuName = "Game/Recipe")]
public class RecipeData : ScriptableObject
{
    [Header("配方唯一ID (可选，便于调试)")]
    public string recipeID;

    [Header("材料槽 A (必填)")]
    public RecipeIngredient ingredientA;

    [Header("材料槽 B (留空 itemID 表示单材料配方)")]
    public RecipeIngredient ingredientB;

    [Header("产出")]
    public ItemData resultItem;
    public int resultAmount = 1;
}
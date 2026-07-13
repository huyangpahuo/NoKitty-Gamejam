/// <summary>
/// One ingredient requirement within a RecipeData: which item, and how
/// many of it. Embedded directly in RecipeData (not a standalone asset).
/// </summary>
[System.Serializable]
public class RecipeIngredient
{
    public ItemData item;
    public int amount = 1;
}
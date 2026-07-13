/// <summary>
/// Tracks the single in-flight drag payload. Only one drag can happen at a
/// time, so this is a static holder rather than something passed through
/// Unity's PointerEventData. Drop targets mark the drag as "consumed" when
/// they actually act on it, so the source can tell an accepted drop apart
/// from a release over empty/invalid space.
/// </summary>
public static class DragContext
{
    public enum SourceKind
    {
        None,
        InventorySlot,
        CraftingSlotA,
        CraftingSlotB
    }

    public static SourceKind Source { get; private set; } = SourceKind.None;
    public static string ItemId { get; private set; }
    public static bool WasConsumed { get; private set; }

    public static bool IsDragging => Source != SourceKind.None;

    public static void Begin(SourceKind source, string itemId)
    {
        Source = source;
        ItemId = itemId;
        WasConsumed = false;
    }

    public static void MarkConsumed()
    {
        WasConsumed = true;
    }

    public static void End()
    {
        Source = SourceKind.None;
        ItemId = null;
        WasConsumed = false;
    }
}
using UnityEngine;

public class PlayerItemInteract : MonoBehaviour
{
    private ItemInteract currentItem;

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.F))
            return;

        // 如果面板已经打开
        // 无论玩家在哪里都允许关闭
        if (ItemUIManager.Instance != null &&
            ItemUIManager.Instance.IsPanelOpen())
        {
            ItemUIManager.Instance.CloseItemInfo();
            return;
        }

        // 面板关闭时才尝试交互
        if (currentItem != null)
        {
            currentItem.Interact();
        }
    }

    public void SetCurrentItem(ItemInteract item)
    {
        currentItem = item;
    }

    public void ClearCurrentItem(ItemInteract item)
    {
        if (currentItem == item)
        {
            currentItem = null;
        }
    }
}
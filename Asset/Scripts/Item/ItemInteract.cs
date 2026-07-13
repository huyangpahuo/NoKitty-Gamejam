using UnityEngine;

public class ItemInteract : MonoBehaviour
{
    [Header("物品数据")]
    [SerializeField] private ItemData itemData;

    [Header("F图标生成点")]
    [SerializeField] private Transform fPoint;

    private StoryItemInteractTrigger storyTrigger;

    private void Awake()
    {
        storyTrigger = GetComponent<StoryItemInteractTrigger>();
    }

    public void Interact()
    {
        if (ItemUIManager.Instance == null)
            return;

        if (storyTrigger != null && storyTrigger.TryPlayStory())
            return;

        AudioManager.Instance.PlaySFX("ClickSoundsClip");

        ItemUIManager.Instance.ShowItemInfo(itemData);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerItemInteract interact =
            other.GetComponent<PlayerItemInteract>();

        if (interact != null)
        {
            interact.SetCurrentItem(this);
        }

        if (ItemUIManager.Instance != null)
        {
            Vector3 iconPosition =
                fPoint != null
                ? fPoint.position
                : transform.position;

            ItemUIManager.Instance.ShowFIcon(iconPosition);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerItemInteract interact =
            other.GetComponent<PlayerItemInteract>();

        if (interact != null)
        {
            interact.ClearCurrentItem(this);
        }

        if (ItemUIManager.Instance != null)
        {
            ItemUIManager.Instance.HideFIcon();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (fPoint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(fPoint.position, 0.1f);
    }
#endif
}
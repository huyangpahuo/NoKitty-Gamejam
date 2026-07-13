using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUIManager : MonoBehaviour
{
    public static ItemUIManager Instance;

    [Header("物品面板")]
    [SerializeField] private GameObject itemInfoPanel;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemDescriptionText;

    [Header("F图标")]
    [SerializeField] private GameObject fIconPrefab;

    private GameObject fIconInstance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        itemInfoPanel.SetActive(false);

        // Tooltip-style panels must never intercept the pointer themselves —
        // if they did, showing the panel under the cursor would steal the
        // raycast hit away from the slot that triggered it, firing
        // OnPointerExit on the slot, hiding the panel, re-exposing the slot,
        // firing OnPointerEnter again, and so on every frame. Disabling
        // raycastTarget on the panel's graphics keeps pointer events passing
        // through to whatever's underneath.
        DisableRaycastBlocking();

        // 保持图片比例，类似 StoryPlayer
        if (itemIcon != null)
        {
            itemIcon.preserveAspect = true;
        }

        if (fIconPrefab != null)
        {
            fIconInstance = Instantiate(fIconPrefab);
            fIconInstance.SetActive(false);
        }
    }

    private void DisableRaycastBlocking()
    {
        // Catch every Graphic under the panel (icon, texts, background
        // panel image, anything else added later) rather than only the
        // explicitly serialized ones, so this stays correct even if the
        // panel's hierarchy changes later.
        Graphic[] graphics = itemInfoPanel.GetComponentsInChildren<Graphic>(true);
        foreach (Graphic graphic in graphics)
        {
            graphic.raycastTarget = false;
        }
    }

    public void ShowFIcon(Vector3 worldPosition)
    {
        if (fIconInstance == null)
            return;

        fIconInstance.transform.position = worldPosition;
        fIconInstance.SetActive(true);
    }

    public void HideFIcon()
    {
        if (fIconInstance == null)
            return;

        fIconInstance.SetActive(false);
    }

    public void ShowItemInfo(ItemData itemData)
    {
        if (itemData == null)
            return;

        itemInfoPanel.SetActive(true);

        itemNameText.text = itemData.itemName;
        itemDescriptionText.text = itemData.description;

        if (itemIcon != null)
        {
            itemIcon.sprite = itemData.icon;

            // 再次确保保持比例
            itemIcon.preserveAspect = true;

            // 没有图标时隐藏
            itemIcon.enabled = itemData.icon != null;
        }
    }

    public bool IsPanelOpen()
    {
        return itemInfoPanel.activeSelf;
    }

    public void CloseItemInfo()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("ClickSoundsClip");
        }

        itemInfoPanel.SetActive(false);
    }

    public void NoSoundCloseItemInfo()
    {
        itemInfoPanel.SetActive(false);
    }
}
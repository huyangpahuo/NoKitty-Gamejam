using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SubmissionPanelUI : MonoBehaviour
{
    [Header("UI Refs")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private Button submitButton;

    [Header("Text Content")]
    [SerializeField, TextArea] private string text01_HasItem = "你已拥有该物品，可以提交。";
    [SerializeField, TextArea] private string text02_NoItem = "你还没有该物品，无法提交。";
    [SerializeField, TextArea] private string textSubmitted = "该物品已提交。";

    private ItemData _requiredItem;
    private string _submissionId;
    private ItemSubmissionInteract _currentSource;

    private void Awake()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
        if (submitButton != null) submitButton.onClick.AddListener(OnClickSubmit);
    }

    private void OnDestroy()
    {
        if (submitButton != null) submitButton.onClick.RemoveListener(OnClickSubmit);
    }

    public void Open(ItemSubmissionInteract source, string submissionId, ItemData requiredItem)
    {
        _currentSource = source;
        _submissionId = submissionId;
        _requiredItem = requiredItem;

        if (panelRoot != null) panelRoot.SetActive(true);

        if (itemImage != null)
        {
            itemImage.sprite = requiredItem != null ? requiredItem.icon : null;
            itemImage.enabled = itemImage.sprite != null;
            itemImage.preserveAspect = true;
        }

        RefreshState();
    }

    public void Close()
    {
        _currentSource = null;
        _submissionId = null;
        _requiredItem = null;
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void RefreshState()
    {
        if (submitButton == null || infoText == null) return;

        if (SubmissionStateManager.IsSubmitted(_submissionId))
        {
            infoText.text = textSubmitted;
            submitButton.interactable = false;
            return;
        }

        if (_requiredItem == null || string.IsNullOrEmpty(_requiredItem.itemID))
        {
            infoText.text = "未配置 requiredItem。";
            submitButton.interactable = false;
            return;
        }

        var inv = InventoryManager.Instance;
        bool hasItem = inv != null && inv.GetTotalAmount(_requiredItem.itemID) > 0;

        infoText.text = hasItem ? text01_HasItem : text02_NoItem;
        submitButton.interactable = hasItem;
    }

    private void OnClickSubmit()
    {
        if (SubmissionStateManager.IsSubmitted(_submissionId)) return;
        if (_requiredItem == null || string.IsNullOrEmpty(_requiredItem.itemID)) return;

        var inv = InventoryManager.Instance;
        if (inv == null) return;

        if (inv.GetTotalAmount(_requiredItem.itemID) <= 0)
        {
            RefreshState();
            return;
        }

        int removed = inv.RemoveItem(_requiredItem.itemID, 1);
        if (removed <= 0)
        {
            RefreshState();
            return;
        }

        SubmissionStateManager.MarkSubmitted(_submissionId);
        RefreshState();

        if (_currentSource != null)
            _currentSource.OnSubmittedSuccessfully();
    }
}
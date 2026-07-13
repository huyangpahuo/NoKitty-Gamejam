using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemSubmissionInteract : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private string submissionId = "submission_001";
    [SerializeField] private ItemData requiredItem;

    [Header("F Icon")]
    [SerializeField] private Transform point;
    [SerializeField] private GameObject fIconPrefab;

    [Header("Panel")]
    [SerializeField] private SubmissionPanelUI panelUI;

    [Header("Rewards (granted after submit)")]
    [SerializeField] private SubmissionReward[] rewards;

    private bool _playerInRange;
    private GameObject _fIconInstance;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = true;
        ShowFIconIfNeeded();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInRange = false;
        HideFIcon();

        if (panelUI != null && panelUI.IsOpen)
            panelUI.Close();
    }

    private void Update()
    {
        if (!_playerInRange) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (panelUI == null) return;

            if (panelUI.IsOpen)
            {
                panelUI.Close();
                return;
            }

            if (SubmissionStateManager.IsSubmitted(submissionId))
                return;

            panelUI.Open(this, submissionId, requiredItem);
        }
    }

    private void ShowFIconIfNeeded()
    {
        if (SubmissionStateManager.IsSubmitted(submissionId)) return;
        if (fIconPrefab == null) return;

        Vector3 pos = point != null ? point.position : transform.position;

        if (_fIconInstance == null)
            _fIconInstance = Instantiate(fIconPrefab, pos, Quaternion.identity);
        else
        {
            _fIconInstance.transform.position = pos;
            _fIconInstance.SetActive(true);
        }
    }

    private void HideFIcon()
    {
        if (_fIconInstance != null)
            _fIconInstance.SetActive(false);
    }

    public void OnSubmittedSuccessfully()
    {
        GrantRewards();
        HideFIcon();
    }

    public void GrantRewards()
    {
        var inv = InventoryManager.Instance;
        if (inv == null || rewards == null) return;

        foreach (var r in rewards)
        {
            if (r == null || r.item == null) continue;
            if (string.IsNullOrEmpty(r.item.itemID) || r.amount <= 0) continue;

            int leftover = inv.AddItem(r.item.itemID, r.amount);
            if (leftover > 0)
            {
                Debug.LogWarning(
                    $"[ItemSubmissionInteract] Reward overflow: itemId={r.item.itemID}, give={r.amount}, leftover={leftover}");
            }
        }
    }
}
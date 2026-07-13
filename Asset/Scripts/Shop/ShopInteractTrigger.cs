using UnityEngine;

/// <summary>
/// 挂在场景商店物体上：
/// 玩家进入触发器显示F，按F开/关商店面板（并注入该商店规则）
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ShopInteractTrigger : MonoBehaviour
{
    [Header("【商店配置】该商店使用的规则")]
    [SerializeField] private ShopRuleData shopRule;

    [Header("【UI】场景中的商店面板")]
    [SerializeField] private ShopPanelUI shopPanelUI;

    [Header("【F图标】生成点（可空，空则用自身位置）")]
    [SerializeField] private Transform point;

    [Header("【F图标】预制体")]
    [SerializeField] private GameObject fIconPrefab;

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
        ShowFIcon();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        _playerInRange = false;
        HideFIcon();

        if (shopPanelUI != null && shopPanelUI.IsOpen)
            shopPanelUI.ClosePanel();
    }

    private void Update()
    {
        if (!_playerInRange) return;
        if (!Input.GetKeyDown(KeyCode.F)) return;

        if (shopPanelUI == null) return;
        if (shopRule == null) return;

        shopPanelUI.ToggleWithRule(shopRule);
    }

    private void ShowFIcon()
    {
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
}
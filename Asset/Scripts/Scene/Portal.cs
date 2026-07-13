using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("目标场景（留空 = 同场景传送）")]
    [SerializeField] private string targetScene;

    [Header("目标传送点ID")]
    [SerializeField] private string targetPointId;

    [Header("E图标位置（可选）")]
    [SerializeField] private Transform ePoint;

    [Header("E图标预制体")]
    [SerializeField] private GameObject eIconPrefab;

    private GameObject eIconInstance;
    private bool playerInside;

    private void Update()
    {
        if (!playerInside) return;
        if (TransitionManager.Instance.IsTransitioning) return;
        if (!Input.GetKeyDown(KeyCode.E)) return;

        Vector2 center = Camera.main.WorldToViewportPoint(transform.position);

        if (string.IsNullOrEmpty(targetScene))
        {
            TeleportPoint point = FindTargetPoint(targetPointId);
            PlayerController player = FindFirstObjectByType<PlayerController>();

            if (point == null)
            {
                Debug.LogWarning($"Portal: 找不到传送点 \"{targetPointId}\"");
                return;
            }
            if (player == null)
            {
                Debug.LogWarning("Portal：场景中没有找到 PlayerController。");
                return;
            }

            StartCoroutine(
                TransitionManager.Instance.Teleport(
                    player.transform,
                    point.transform.position,
                    center));
        }
        else
        {
            StartCoroutine(
                TransitionManager.Instance.LoadScene(
                    targetScene,
                    targetPointId,
                    center));
        }
    }

    private TeleportPoint FindTargetPoint(string id)
    {
        foreach (TeleportPoint p in FindObjectsOfType<TeleportPoint>())
        {
            if (p.PointId == id)
                return p;
        }

        return null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        ShowEIcon();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        HideEIcon();
    }

    private void ShowEIcon()
    {
        if (eIconPrefab == null)
            return;

        if (eIconInstance != null)
            return;

        Transform point = ePoint != null ? ePoint : transform;

        eIconInstance = Instantiate(
            eIconPrefab,
            point.position,
            Quaternion.identity,
            point);
    }

    private void HideEIcon()
    {
        if (eIconInstance == null)
            return;

        Destroy(eIconInstance);
        eIconInstance = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Transform point = ePoint != null ? ePoint : transform;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(point.position, 0.1f);
    }
#endif
}
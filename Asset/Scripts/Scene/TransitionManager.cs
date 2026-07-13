using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("UI")]
    [SerializeField] private Image transitionImage;

    [Header("动画")]
    [SerializeField] private float transitionDuration = 0.35f;

    [SerializeField] private float maxRadius = 2f;

    [SerializeField]
    private AnimationCurve closeCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    [SerializeField]
    private AnimationCurve openCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Material runtimeMaterial;

    private bool isTransitioning;

    public bool IsTransitioning => isTransitioning;

    private static readonly int RadiusID =
        Shader.PropertyToID("_Radius");

    private static readonly int CenterID =
        Shader.PropertyToID("_Center");

    private static readonly int AspectID =
        Shader.PropertyToID("_Aspect");

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        runtimeMaterial =
            Instantiate(transitionImage.material);

        transitionImage.material =
            runtimeMaterial;

        runtimeMaterial.SetFloat(
            RadiusID,
            maxRadius);

        transitionImage.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (runtimeMaterial != null)
        {
            runtimeMaterial.SetFloat(
                AspectID,
                (float)Screen.width / Screen.height);
        }
    }

    public IEnumerator Close(Vector2 viewportCenter)
    {
        transitionImage.gameObject.SetActive(true);

        runtimeMaterial.SetVector(
            CenterID,
            new Vector4(
                viewportCenter.x,
                viewportCenter.y,
                0,
                0));

        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / transitionDuration);

            runtimeMaterial.SetFloat(
                RadiusID,
                Mathf.Lerp(
                    maxRadius,
                    0f,
                    closeCurve.Evaluate(t)));

            yield return null;
        }

        runtimeMaterial.SetFloat(
            RadiusID,
            0f);
    }

    public IEnumerator Open(Vector2 viewportCenter)
    {
        runtimeMaterial.SetVector(
            CenterID,
            new Vector4(
                viewportCenter.x,
                viewportCenter.y,
                0,
                0));

        float timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / transitionDuration);

            runtimeMaterial.SetFloat(
                RadiusID,
                Mathf.Lerp(
                    0f,
                    maxRadius,
                    openCurve.Evaluate(t)));

            yield return null;
        }

        runtimeMaterial.SetFloat(
            RadiusID,
            maxRadius);

        transitionImage.gameObject.SetActive(false);
    }

    public IEnumerator Teleport(
    Transform player,
    Vector3 targetPosition,
    Vector2 viewportCenter)
    {
        if (isTransitioning)
            yield break;

        isTransitioning = true;

        try
        {
            yield return Close(viewportCenter);

            Vector3 oldPosition = player.position;

            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.position = targetPosition;     // 同步刚体位置
                Physics2D.SyncTransforms();        // 立即同步Transform，避免插值残影
            }
            else
            {
                player.position = targetPosition;
            }

            CinemachineVirtualCamera vcam =
                FindObjectOfType<CinemachineVirtualCamera>();

            if (vcam != null)
            {
                vcam.OnTargetObjectWarped(
                    player,
                    targetPosition - oldPosition);
            }

            // 等一帧，让Cinemachine在LateUpdate里真正应用位移
            yield return null;

            // 用相机warp后的新位置重新计算中心点，而不是复用旧的
            Vector2 newCenter =
                Camera.main.WorldToViewportPoint(targetPosition);

            yield return Open(newCenter);
        }
        finally
        {
            isTransitioning = false;
        }
    }

    public IEnumerator LoadScene(
    string sceneName,
    string targetPointId,
    Vector2 closeCenter)
    {
        if (isTransitioning)
            yield break;

        isTransitioning = true;

        try
        {
            yield return Close(closeCenter);

            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

            while (!op.isDone)
                yield return null;

            yield return null; // 让新场景里物体的 Awake/Start 跑完

            Vector2 openCenter = new Vector2(0.5f, 0.5f);

            TeleportPoint target = null;
            foreach (TeleportPoint p in FindObjectsOfType<TeleportPoint>())
            {
                if (p.PointId == targetPointId) { target = p; break; }
            }

            PlayerController playerController = FindFirstObjectByType<PlayerController>();
            if (target != null && playerController != null)
            {
                Transform player = playerController.transform;
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    rb.position = target.transform.position;
                    Physics2D.SyncTransforms();
                }
                else
                {
                    player.position = target.transform.position;
                }

                CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
                if (vcam != null)
                {
                    vcam.Follow = player;             // 新场景的vcam需要重新指定Follow
                    vcam.OnTargetObjectWarped(player, Vector3.zero); // 强制立即对齐，不做阻尼
                }

                yield return null; // 等Cinemachine在LateUpdate里应用对齐

                openCenter = Camera.main.WorldToViewportPoint(target.transform.position);
            }
            else
            {
                Debug.LogWarning(
                    $"LoadScene: 找不到传送点 \"{targetPointId}\" 或玩家实例，使用屏幕中心展开");
            }

            yield return Open(openCenter);
        }
        finally
        {
            isTransitioning = false;
        }
    }
}
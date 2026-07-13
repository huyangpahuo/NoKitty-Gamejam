using DG.Tweening;
using TMPro;
using UnityEngine;

public class WorldMessageUI : MonoBehaviour
{
    public static WorldMessageUI Instance;

    [Header("提示UI预制体")]
    [SerializeField] private GameObject messagePrefab;

    [Header("显示位置")]
    [SerializeField] private Transform uiRoot;

    private GameObject messageInstance;

    private CanvasGroup canvasGroup;

    private TMP_Text messageText;

    private Sequence currentSequence;

    private void Awake()
    {
        Instance = this;

        if (messagePrefab == null)
            return;

        messageInstance =
            Instantiate(messagePrefab, uiRoot);

        canvasGroup =
            messageInstance.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup =
                messageInstance.AddComponent<CanvasGroup>();
        }

        messageText =
            messageInstance.GetComponentInChildren<TMP_Text>();

        canvasGroup.alpha = 0;

        messageInstance.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        if (messageInstance == null)
            return;

        if (currentSequence != null)
        {
            currentSequence.Kill();
        }

        messageInstance.SetActive(true);

        if (messageText != null)
        {
            messageText.text = message;
        }

        canvasGroup.alpha = 0;

        RectTransform rect =
            messageInstance.GetComponent<RectTransform>();

        Vector2 startPos = Vector2.zero;

        if (rect != null)
        {
            startPos = rect.anchoredPosition;

            rect.anchoredPosition =
                startPos - Vector2.up * 20f;
        }

        currentSequence = DOTween.Sequence();

        currentSequence.Append(
            canvasGroup.DOFade(1f, 0.25f)
        );

        if (rect != null)
        {
            currentSequence.Join(
                rect.DOAnchorPos(
                    startPos,
                    0.25f
                )
            );
        }

        currentSequence.AppendInterval(2f);

        currentSequence.Append(
            canvasGroup.DOFade(0f, 0.4f)
        );

        currentSequence.OnComplete(() =>
        {
            messageInstance.SetActive(false);
        });
    }

    private void OnDestroy()
    {
        if (currentSequence != null)
        {
            currentSequence.Kill();
        }
    }
}
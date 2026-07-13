using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonHoverScale : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float duration = 0.15f;

    private Vector3 originalScale;
    private Tween scaleTween;
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (rectTransform == null)
            return;

        scaleTween?.Kill();

        scaleTween = rectTransform
            .DOScale(originalScale * hoverScale, duration)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (rectTransform == null)
            return;

        scaleTween?.Kill();

        scaleTween = rectTransform
            .DOScale(originalScale, duration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void OnDisable()
    {
        scaleTween?.Kill();
    }

    private void OnDestroy()
    {
        scaleTween?.Kill();
    }
}
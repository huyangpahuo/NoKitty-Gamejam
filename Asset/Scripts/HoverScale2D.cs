using UnityEngine;
using DG.Tweening;

public class HoverScale2D : MonoBehaviour
{
    [Header("缩放设置")]
    [SerializeField] private float hoverScale = 1.2f;
    [SerializeField] private float duration = 0.15f;

    private Vector3 originalScale;
    private Tween scaleTween;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void OnMouseEnter()
    {
        Debug.Log("ENTER");
        ScaleTo(originalScale * hoverScale);
    }

    private void OnMouseExit()
    {
        ScaleTo(originalScale);
    }

    private void ScaleTo(Vector3 target)
    {
        scaleTween?.Kill();

        scaleTween = transform.DOScale(target, duration)
            .SetEase(Ease.OutBack);
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
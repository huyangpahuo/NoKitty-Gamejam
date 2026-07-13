using UnityEngine;
using DG.Tweening;

public class GlowItem : MonoBehaviour
{
    [Header("Glow")]

    [Range(0f, 20f)]
    [SerializeField] private float enterGlowIntensity = 5f;

    [Range(0f, 20f)]
    [SerializeField] private float exitGlowIntensity = 0f;

    [Header("Animation")]

    [Range(0.01f, 2f)]
    [SerializeField] private float duration = 0.2f;

    private Material mat;
    private Tween glowTween;

    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            // 获取实例材质，避免影响其他对象
            mat = sr.material;
        }

        if (mat != null)
        {
            mat.SetFloat("_GlowIntensity", exitGlowIntensity);
        }
    }

    private void OnMouseEnter()
    {
        if (mat == null) return;

        glowTween?.Kill();

        glowTween = DOTween.To(
            () => mat.GetFloat("_GlowIntensity"),
            value => mat.SetFloat("_GlowIntensity", value),
            enterGlowIntensity,
            duration
        );
    }

    private void OnMouseExit()
    {
        if (mat == null) return;

        glowTween?.Kill();

        glowTween = DOTween.To(
            () => mat.GetFloat("_GlowIntensity"),
            value => mat.SetFloat("_GlowIntensity", value),
            exitGlowIntensity,
            duration
        );
    }

    private void OnDestroy()
    {
        glowTween?.Kill();
    }
}
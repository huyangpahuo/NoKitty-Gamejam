using TMPro;
using UnityEngine;
using System.Collections;

public class AnimatedNumberText : MonoBehaviour
{
    [Header("【显示】数字文本组件")]
    [SerializeField] private TMP_Text text;

    [Header("【动画】数字变化时长（秒）")]
    [Tooltip("建议小于 1 秒，例如 0.35")]
    [SerializeField] private float duration = 0.35f;

    private Coroutine _co;
    private int _shown;

    public void SetImmediate(int value)
    {
        _shown = value;
        if (text != null) text.text = value.ToString();
    }

    public void AnimateTo(int target)
    {
        if (text == null) return;
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(CoAnimate(_shown, target));
    }

    private IEnumerator CoAnimate(int from, int to)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            _shown = Mathf.RoundToInt(Mathf.Lerp(from, to, k));
            text.text = _shown.ToString();
            yield return null;
        }
        _shown = to;
        text.text = _shown.ToString();
        _co = null;
    }
}
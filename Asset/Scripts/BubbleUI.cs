using System.Collections;
using UnityEngine;

public class BubbleUI : MonoBehaviour
{
    [Header("左气泡")]
    public GameObject leftBubble;

    [Header("右气泡")]
    public GameObject rightBubble;

    [Header("显示时间")]
    public float showTime = 1.5f;

    [Header("淡入淡出时间")]
    public float fadeTime = 0.25f;

    private Coroutine currentRoutine;

    public void ShowLeft()
    {
        ShowBubble(leftBubble, rightBubble);
    }

    public void ShowRight()
    {
        ShowBubble(rightBubble, leftBubble);
    }

    private void ShowBubble(GameObject target, GameObject other)
    {
        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(BubbleRoutine(target, other));
    }

    private IEnumerator BubbleRoutine(GameObject target, GameObject other)
    {
        CanvasGroup targetGroup = GetGroup(target);
        CanvasGroup otherGroup = GetGroup(other);

        // 关闭另一边
        otherGroup.alpha = 0;
        other.SetActive(false);

        // 播放音效
        AudioManager.Instance.PlaySFX("CatMeowClip");

        // 显示当前
        target.SetActive(true);

        float t = 0;

        // Fade In
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            targetGroup.alpha = t / fadeTime;
            yield return null;
        }

        targetGroup.alpha = 1;

        yield return new WaitForSeconds(showTime);

        t = 0;

        // Fade Out
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            targetGroup.alpha = 1 - t / fadeTime;
            yield return null;
        }

        targetGroup.alpha = 0;
        target.SetActive(false);
    }

    private CanvasGroup GetGroup(GameObject obj)
    {
        CanvasGroup group = obj.GetComponent<CanvasGroup>();

        if (group == null)
            group = obj.AddComponent<CanvasGroup>();

        return group;
    }
}
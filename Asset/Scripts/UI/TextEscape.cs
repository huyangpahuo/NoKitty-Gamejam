using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TextEscape : MonoBehaviour
{
    [Header("检测范围")]
    public Vector2 detectSize = new Vector2(200, 80);

    [Header("逃跑距离")]
    public float escapeDistance = 100f;

    [Header("移动速度")]
    public float moveSpeed = 10f;

    private RectTransform rect;
    private Vector2 originalPos;
    private Vector2 targetPos;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();

        originalPos = rect.anchoredPosition;
        targetPos = originalPos;
    }

    private void Update()
    {
        Vector2 mouse = Input.mousePosition;

        Vector2 center =
            RectTransformUtility.WorldToScreenPoint(null, rect.position);

        bool inside =
            mouse.x > center.x - detectSize.x * 0.5f &&
            mouse.x < center.x + detectSize.x * 0.5f &&
            mouse.y > center.y - detectSize.y * 0.5f &&
            mouse.y < center.y + detectSize.y * 0.5f;

        if (inside)
        {
            Vector2 dir = (center - mouse).normalized;

            targetPos = originalPos + dir * escapeDistance;
        }
        else
        {
            targetPos = originalPos;
        }

        rect.anchoredPosition =
            Vector2.Lerp(rect.anchoredPosition,
                         targetPos,
                         moveSpeed * Time.deltaTime);
    }
}

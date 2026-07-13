using UnityEngine;
using UnityEngine.Events;

public class AutoRotate : MonoBehaviour
{
    [Header("转速(度/秒)")]
    public float rotateSpeed = 90f;

    [Header("顺时针")]
    public bool clockwise = true;

    [Header("是否正在旋转")]
    public bool isRotating = true;

    [Header("指针")]
    public Transform pointer;

    [Header("左半边事件")]
    public UnityEvent onLeftSide;

    [Header("右半边事件")]
    public UnityEvent onRightSide;

    [Header("按钮按下音效Prefab")]
    [SerializeField] private AudioSource buttonPressSfx;

    [Header("设置UI")]
    private SettingsUI settingsUI;

    private void Start()
    {
        settingsUI = FindObjectOfType<SettingsUI>();
    }

    private void Update()
    {
        if (!isRotating)
            return;

        float dir = clockwise ? -1 : 1;

        transform.Rotate(0, 0, rotateSpeed * dir * Time.deltaTime);
    }

    private void OnMouseDown()
    {
        // 设置菜单打开时禁止交互
        if (settingsUI != null && settingsUI.IsOpen())
        {
            return;
        }

        PlayButtonSfx();
        ToggleRotation();
    }

    private void PlayButtonSfx()
    {
        if (buttonPressSfx == null)
        {
            Debug.LogWarning($"{gameObject.name}: AutoRotate 未指定按钮音效Prefab");
            return;
        }

        AudioManager.Instance.PlaySFX(buttonPressSfx);
    }

    public void ToggleRotation()
    {
        isRotating = !isRotating;

        if (!isRotating)
        {
            CheckPointerPosition();
        }
    }

    private void CheckPointerPosition()
    {
        float x = pointer.position.x;

        Debug.Log($"Pointer X = {x}");

        if (x < transform.position.x)
        {
            Debug.Log("左侧");
            onLeftSide?.Invoke();
        }
        else
        {
            Debug.Log("右侧");
            onRightSide?.Invoke();
        }
    }
}
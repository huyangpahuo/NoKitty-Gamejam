using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Sole owner of the floating drag-preview icon. Lives on its own object
/// under the top-level Canvas so it renders above every slot regardless of
/// scroll view clipping. Slots themselves never move during a drag — only
/// this icon follows the mouse.
/// </summary>
public class DragIconController : MonoBehaviour
{
    public static DragIconController Instance { get; private set; }

    [SerializeField] private RectTransform iconTransform;
    [SerializeField] private Image iconImage;
    [SerializeField] private Canvas rootCanvas;

    private bool _isActive;

    private void Awake()
    {
        Instance = this;

        if (iconImage != null)
            iconImage.preserveAspect = true;   // <- important
        Hide();
    }

    public void Begin(Sprite sprite)
    {
        iconImage.sprite = sprite;
        iconImage.enabled = sprite != null;
        _isActive = true;
        iconTransform.gameObject.SetActive(true);
        Follow();
    }

    private void Update()
    {
        if (_isActive)
            Follow();
    }

    private void Follow()
    {
        Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)rootCanvas.transform, Input.mousePosition, cam, out Vector2 localPoint);

        iconTransform.localPosition = localPoint;
    }

    public void Hide()
    {
        _isActive = false;
        iconImage.sprite = null;
        iconImage.enabled = false;
        iconTransform.gameObject.SetActive(false);
    }
}
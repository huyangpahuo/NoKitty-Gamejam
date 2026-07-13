#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class ToWorld : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    private SceneAsset sceneAsset;
#endif

    [SerializeField]
    private string sceneName;

    private SettingsUI settingsUI;
    private WorldLock worldLock;

    [Header("交互提示（可选）")]
    [SerializeField]
    private GameObject promptPrefab;

    [SerializeField]
    private Transform point;

    private GameObject promptInstance;
    private bool playerInRange;

    private void Start()
    {
        settingsUI = FindFirstObjectByType<SettingsUI>();
        worldLock = GetComponent<WorldLock>();
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            LoadScene();
        }
    }

    private void OnValidate()
    {
#if UNITY_EDITOR
        if (sceneAsset != null)
        {
            sceneName = sceneAsset.name;
        }
#endif
    }

    /// <summary>
    /// 是否允许进入场景
    /// </summary>
    private bool CanLoadScene()
    {
        if (settingsUI != null &&
            settingsUI.IsOpen())
        {
            return false;
        }

        if (worldLock != null &&
            worldLock.IsLocked)
        {
            WorldMessageUI.Instance?.ShowMessage(
                "此区域尚未解锁"
            );

            return false;
        }

        return true;
    }

    /// <summary>
    /// 加载场景（可直接给 UI Button 的 OnClick 使用）
    /// </summary>
    public void LoadScene()
    {
        if (!CanLoadScene())
            return;

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        LoadScene();
    }

    private void OnMouseEnter()
    {
        if (settingsUI != null &&
            settingsUI.IsOpen())
        {
            return;
        }

        if (worldLock != null &&
            worldLock.IsLocked)
        {
            return;
        }

        AudioManager.Instance?.PlaySFX("UIButtonClickClip");
    }

    /// <summary>
    /// 显示交互提示
    /// </summary>
    public void ShowPrompt()
    {
        if (promptPrefab == null)
            return;

        if (promptInstance != null)
            return;

        Transform spawnPoint = point != null ? point : transform;

        promptInstance = Instantiate(
            promptPrefab,
            spawnPoint.position,
            Quaternion.identity);
    }

    /// <summary>
    /// 隐藏交互提示
    /// </summary>
    public void HidePrompt()
    {
        if (promptInstance == null)
            return;

        Destroy(promptInstance);
        promptInstance = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        ShowPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        HidePrompt();
    }

    private void OnDisable()
    {
        playerInRange = false;
        HidePrompt();
    }
}
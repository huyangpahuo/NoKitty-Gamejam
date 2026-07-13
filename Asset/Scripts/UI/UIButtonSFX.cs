using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIButtonSFX : MonoBehaviour
{
    [Header("点击音效Prefab")]
    [SerializeField] private AudioSource sfxPrefab;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(Play);
    }

    private void Play()
    {
        if (sfxPrefab == null)
        {
            Debug.LogWarning($"{gameObject.name}: UIButtonSFX 未指定音效Prefab");
            return;
        }

        AudioManager.Instance.PlaySFX(sfxPrefab);
    }
}
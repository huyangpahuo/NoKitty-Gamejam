using UnityEngine;

public class WorldLock : MonoBehaviour
{
    [Header("关卡ID")]
    [SerializeField]
    private string levelId;

    [Header("锁图标预制体")]
    [SerializeField]
    private GameObject lockPrefab;

    [Header("锁生成点")]
    [SerializeField]
    private Transform lockPoint;

    private GameObject lockInstance;

    public bool IsLocked { get; private set; }

    private void Start()
    {
        IsLocked =
            !SettingsManager.IsLevelUnlocked(levelId);

        if (IsLocked)
        {
            SpawnLock();
        }
    }

    private void SpawnLock()
    {
        if (lockPrefab == null)
            return;

        Vector3 pos =
            lockPoint != null
            ? lockPoint.position
            : transform.position;

        lockInstance =
            Instantiate(lockPrefab, pos, Quaternion.identity);

        lockInstance.transform.SetParent(transform);
    }

    public void Unlock()
    {
        IsLocked = false;

        SettingsManager.UnlockLevel(levelId);

        if (lockInstance != null)
        {
            Destroy(lockInstance);
        }
    }
}
using UnityEngine;
using Cinemachine;

public class PlayerSpawnPoint : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private void Start()
    {
        GameObject player = Instantiate(
            playerPrefab,
            transform.position,
            Quaternion.identity);

        CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();

        if (vcam != null)
        {
            vcam.Follow = player.transform;
        }
    }
}
using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    [SerializeField] private string pointId;

    public string PointId => pointId;
}
using System.Collections.Generic;
using UnityEngine;

public class TeleportZone : MonoBehaviour
{
    [SerializeField] private Transform exitPoint;
    private HashSet<GameObject> recentlyTeleported = new HashSet<GameObject>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = other.gameObject;

        if (recentlyTeleported.Contains(obj))
            return;

        other.transform.position = exitPoint.position;

        TeleportZone exitZone;
        if (exitPoint.TryGetComponent<TeleportZone>(out exitZone))
        {
            exitZone.recentlyTeleported.Add(obj);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        recentlyTeleported.Remove(other.gameObject);
    }
}
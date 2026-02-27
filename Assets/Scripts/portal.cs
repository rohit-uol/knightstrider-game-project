using System.Collections.Generic;
using UnityEngine;
using TheMasterPath.Utilities;

public class TeleportZone : MonoBehaviour
{
    [SerializeField] private Transform exitPoint;
    private HashSet<GameObject> recentlyTeleported = new HashSet<GameObject>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = other.gameObject;

        if (recentlyTeleported.Contains(obj))
            return;

        // Check if the object has  Movement script
        if (obj.TryGetComponent<TheMasterPath.Movement>(out var movement))
        {
            movement.Teleport(exitPoint.position);
            int currentQuadrant = NavigationUtils.GetQuadrant(transform.position);
            MapDestroyer.Instance.HideQuadrant(currentQuadrant);
        }
        else
        {
            // Fallback for objects without the script
            return;
        }

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
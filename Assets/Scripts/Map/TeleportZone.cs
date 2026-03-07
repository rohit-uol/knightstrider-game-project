using System.Collections.Generic;
using UnityEngine;
using TheMasterPath.Utilities;

public class TeleportZone : MonoBehaviour
{
    [SerializeField] private Transform exitPoint;
    private HashSet<GameObject> recentlyTeleported = new HashSet<GameObject>();
    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameObject obj = other.gameObject;

        if (recentlyTeleported.Contains(obj))
            return;

        // Check if the object has  Movement script
        if (obj.TryGetComponent<TheMasterPath.Movement>(out var movement))
        {
            movement.Teleport(exitPoint.position);
            if (!_triggered)
            {
                string parentTag = transform.parent != null ? transform.parent.tag : "";
                if (parentTag.StartsWith("Q") && int.TryParse(parentTag.Substring(1), out int currentQuadrant))
                {
                    MapDestroyer.Instance.HideQuadrant(currentQuadrant);
                }
                _triggered = true;
            }
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
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    void LateUpdate()
    {
        if(Camera.main != null)
        {
            // Position ahead + camera forward direction
            Vector3 targetPos = transform.position + Camera.main.transform.rotation * Vector3.forward;
            // Use camera up to avoid roll/tilting
            transform.LookAt(targetPos, Camera.main.transform.up);
        }
    }
}

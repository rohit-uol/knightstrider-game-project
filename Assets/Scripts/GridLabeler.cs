using UnityEngine;
using UnityEditor;

public class GridLabeler : MonoBehaviour
{
    public Color labelColor = Color.yellow;

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Camera cam = Camera.current;
        if (cam == null) return;

        // Get camera bounds in world space
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        float left = cam.transform.position.x - (width / 2f);
        float right = cam.transform.position.x + (width / 2f);
        float top = cam.transform.position.y + (height / 2f);
        float bottom = cam.transform.position.y - (height / 2f);

        Handles.BeginGUI();
        GUI.color = labelColor;

        // Label Horizontal Grid (Bottom Edge)
        for (int x = Mathf.CeilToInt(left); x <= Mathf.FloorToInt(right); x++)
        {
            Vector3 worldPos = new Vector3(x, bottom + 0.5f, 0);
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);
            GUI.Label(new Rect(screenPos.x - 10, screenPos.y, 50, 20), x.ToString());
        }

        // Label Vertical Grid (Left Edge)
        for (int y = Mathf.CeilToInt(bottom); y <= Mathf.FloorToInt(top); y++)
        {
            Vector3 worldPos = new Vector3(left + 0.5f, y, 0);
            Vector2 screenPos = HandleUtility.WorldToGUIPoint(worldPos);
            GUI.Label(new Rect(screenPos.x, screenPos.y - 10, 50, 20), y.ToString());
        }

        Handles.EndGUI();
    }
#endif
}
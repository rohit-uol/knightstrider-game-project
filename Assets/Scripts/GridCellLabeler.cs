using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GridCellLabeler : MonoBehaviour
{
    public Grid grid;
    public int rangeX = 10;
    public int rangeY = 10;
    public Color textColor = Color.white;

    private void OnDrawGizmos()
    {
        if (grid == null)
        {
            grid = GetComponent<Grid>();
            if (grid == null) return;
        }

        GUIStyle style = new GUIStyle();
        style.normal.textColor = textColor;
        style.alignment = TextAnchor.MiddleCenter;

        // Iterate through the grid range
        for (int x = -rangeX; x <= rangeX; x++)
        {
            for (int y = -rangeY; y <= rangeY; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                // Get the world center of the specific cell
                Vector3 worldPos = grid.GetCellCenterWorld(cellPosition);

#if UNITY_EDITOR
                // Use Handles to draw the text in the Scene View
                string label = $"{x},{y}";
                Handles.Label(worldPos, label, style);
#endif
            }
        }
    }
}
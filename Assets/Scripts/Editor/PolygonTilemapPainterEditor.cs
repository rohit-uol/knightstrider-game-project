using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PolygonTilemapPainter))]
public class PolygonTilemapPainterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PolygonTilemapPainter painter = (PolygonTilemapPainter)target;

        if(GUILayout.Button("Paint Tiles Now"))
        {
            painter.PaintTilesFromPolygon();
        }
    }
}

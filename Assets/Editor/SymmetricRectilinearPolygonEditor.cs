using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(SymmetricRectilinearPolygon))]
public class SymmetricRectilinearPolygonEditor : Editor
{
    private SymmetricRectilinearPolygon poly;
    private const float GridSize = 1f;   // 1-unit snapping

    void OnEnable()
    {
        poly = (SymmetricRectilinearPolygon)target;
    }

    void OnSceneGUI()
    {
        if(poly?.vertexData == null || poly.vertexData.quadrantVertexCount == 0)
            return;

        Transform handleTransform = poly.transform;
        Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ?
                                    handleTransform.rotation : Quaternion.identity;

        Undo.RecordObject(poly.vertexData, "Move Q1 Vertices");
        EditorGUI.BeginChangeCheck();

        Vector2[] q1Verts = poly.vertexData.GetVertices();

        // Draw and move Q1 vertices
        for(int i = 0; i < poly.vertexData.quadrantVertexCount; i++)
        {
            Vector3 localPos;
            Vector3 newWorldPos;

            float handleSize = HandleUtility.GetHandleSize(
                handleTransform.TransformPoint(q1Verts[i])
            ) * 0.08f;

            if(i == 0)  // FIRST VERTEX: bottom edge only (y=0, x snapped)
            {
                Handles.color = Color.red;
                localPos = new Vector3(q1Verts[0].x, 0f, 0f);
                Vector3 worldPos = handleTransform.TransformPoint(localPos);

                newWorldPos = Handles.FreeMoveHandle(
                    worldPos,
                    handleSize,
                    Vector3.right,
                    Handles.SphereHandleCap
                );

                if(worldPos != newWorldPos)
                {
                    Vector3 newLocal = handleTransform.InverseTransformPoint(newWorldPos);
                    float x = Mathf.Clamp(newLocal.x, -poly.Size, 0f);
                    x = Mathf.Round(x / GridSize) * GridSize;
                    poly.vertexData.SetVertex(0, new Vector2(x, 0f));
                }
            }
            else if(i == poly.vertexData.quadrantVertexCount - 1)  // LAST VERTEX: right edge only (x=0, y snapped)
            {
                Handles.color = Color.red;
                localPos = new Vector3(0f, q1Verts[i].y, 0f);
                Vector3 worldPos = handleTransform.TransformPoint(localPos);

                newWorldPos = Handles.FreeMoveHandle(
                    worldPos,
                    handleSize,
                    Vector3.up,
                    Handles.SphereHandleCap
                );

                if(worldPos != newWorldPos)
                {
                    Vector3 newLocal = handleTransform.InverseTransformPoint(newWorldPos);
                    float y = Mathf.Clamp(newLocal.y, 0f, poly.Size);
                    y = Mathf.Round(y / GridSize) * GridSize;
                    poly.vertexData.SetVertex(i, new Vector2(0f, y));
                }
            }
            else  // MIDDLE: full free move + quadrant clamp + grid snap
            {
                Handles.color = Color.cyan;
                localPos = new Vector3(q1Verts[i].x, q1Verts[i].y, 0f);
                Vector3 worldPos = handleTransform.TransformPoint(localPos);

                newWorldPos = Handles.FreeMoveHandle(
                    worldPos,
                    handleSize,
                    Vector3.zero,
                    Handles.SphereHandleCap
                );

                if(worldPos != newWorldPos)
                {
                    Vector3 newLocal = handleTransform.InverseTransformPoint(newWorldPos);
                    float x = Mathf.Clamp(newLocal.x, -poly.Size, 0f);
                    float y = Mathf.Clamp(newLocal.y, 0f, poly.Size);
                    x = Mathf.Round(x / GridSize) * GridSize;
                    y = Mathf.Round(y / GridSize) * GridSize;
                    poly.vertexData.SetVertex(i, new Vector2(x, y));
                }
            }

            // Number labels (green for first/last, white for middle)
            GUIStyle labelStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = (i == 0 || i == poly.vertexData.quadrantVertexCount - 1) ? Color.green : Color.white },
                alignment = TextAnchor.MiddleCenter
            };
            Handles.Label(
                handleTransform.TransformPoint(localPos) + Vector3.up * handleSize * 2f,
                i.ToString(),
                labelStyle
            );
        }

        // Draw full polygon preview
        if(poly.fullVertices != null && poly.fullVertices.Count > 1)
        {
            Handles.color = Color.yellow;
            Vector3[] worldVerts = new Vector3[poly.fullVertices.Count];
            for(int i = 0; i < poly.fullVertices.Count; i++)
            {
                worldVerts[i] = handleTransform.TransformPoint(poly.fullVertices[i]);
            }
            Handles.DrawPolyLine(worldVerts);
        }

        // Draw 4 red quadrant outlines
        DrawQuadrantOutlines(handleTransform, poly.Size);

        if(EditorGUI.EndChangeCheck())
        {
            poly.Rebuild();  // Enforce rectilinear + symmetry
            EditorUtility.SetDirty(poly.vertexData);
            Undo.FlushUndoRecordObjects();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private void DrawQuadrantOutlines(Transform handleTransform, float size)
    {
        Handles.color = Color.purple;

        // TL: x [-size, 0], y [0, size]
        DrawSquare(handleTransform,
            new Vector2(-size, 0f),
            new Vector2(0f, 0f),
            new Vector2(0f, size),
            new Vector2(-size, size));

        // TR: x [0, size], y [0, size]
        DrawSquare(handleTransform,
            new Vector2(0f, 0f),
            new Vector2(size, 0f),
            new Vector2(size, size),
            new Vector2(0f, size));

        // BR: x [0, size], y [-size, 0]
        DrawSquare(handleTransform,
            new Vector2(0f, -size),
            new Vector2(size, -size),
            new Vector2(size, 0f),
            new Vector2(0f, 0f));

        // BL: x [-size, 0], y [-size, 0]
        DrawSquare(handleTransform,
            new Vector2(-size, -size),
            new Vector2(0f, -size),
            new Vector2(0f, 0f),
            new Vector2(-size, 0f));
    }

    private void DrawSquare(Transform t, Vector2 bl, Vector2 br, Vector2 tr, Vector2 tl)
    {
        Vector3 wBL = t.TransformPoint(new Vector3(bl.x, bl.y, 0f));
        Vector3 wBR = t.TransformPoint(new Vector3(br.x, br.y, 0f));
        Vector3 wTR = t.TransformPoint(new Vector3(tr.x, tr.y, 0f));
        Vector3 wTL = t.TransformPoint(new Vector3(tl.x, tl.y, 0f));

        Handles.DrawLine(wBL, wBR);
        Handles.DrawLine(wBR, wTR);
        Handles.DrawLine(wTR, wTL);
        Handles.DrawLine(wTL, wBL);
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if(GUILayout.Button("Force Save"))
        {
            Undo.RecordObject(poly.vertexData, "Manual Save");
            poly.Rebuild();
            EditorUtility.SetDirty(poly.vertexData);
            AssetDatabase.SaveAssets();
        }

        //  New button snapshot latest polygon into PolygonVertexData
        if(GUILayout.Button("Export Polygon Vertices"))
        {
            Undo.RecordObject(poly, "Export Polygon Vertices");
            poly.SaveCurrentVerticesToExportData();
            EditorUtility.SetDirty(poly);
            AssetDatabase.SaveAssets();
        }
    }
}

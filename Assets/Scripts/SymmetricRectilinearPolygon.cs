using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SymmetricRectilinearPolygon : MonoBehaviour
{
    [SerializeField] public Q1VertexData vertexData;  // 
    [SerializeField] PolygonVertexData exportData;   // 
    [SerializeField] float size = 5f;                 // "square" half-size
    float lastSize = -1f;
    int lastVertexCount = -1;                         // Track changes

    const int MaxQ1Vertices = 20; 

    [SerializeField] bool closePolygon = true;

    // Full polygon vertices (all 4 quadrants), local space
    public List<Vector3> fullVertices = new List<Vector3>();

    public LineRenderer lineRenderer;

    // Public getters
    public int QuadrantVertexCount => vertexData ? vertexData.quadrantVertexCount : 0;
    public int VertexCount => QuadrantVertexCount;
    public float Size => size;

    void Reset()
    {
        InitDefaultQ1();
        Rebuild();
    }

    public void InitDefaultQ1()
    {
        float d = Size * 0.3f;

        vertexData.SetVertex(0, new Vector2(-d, 0f));                    // Bottom edge
        vertexData.SetVertex(1, new Vector2(-d, Size * 0.4f));
        vertexData.SetVertex(2, new Vector2(-Size * 0.6f, Size * 0.6f));
        vertexData.SetVertex(3, new Vector2(-Size * 0.8f, Size * 0.8f));
        vertexData.SetVertex(4, new Vector2(0f, Size - d));              // Right edge

        vertexData.quadrantVertexCount = 5;
    }

    public void Rebuild()
    {
        EnforceRectilinear();
        EnforceSymmetryConstraint();
        BuildFourFoldPolygon();

        Debug.Log($"Rebuild: {fullVertices.Count} verts, Size={size}");

        // Update LineRenderer if assigned
        if(lineRenderer != null && fullVertices.Count > 2)
        {
            lineRenderer.positionCount = fullVertices.Count;
            lineRenderer.SetPositions(fullVertices.ToArray());
            Debug.Log($"LineRenderer updated: count={fullVertices.Count}, first={fullVertices[0]}");
        }
        else
        {
            Debug.LogWarning("lineRenderer is NULL!");
        }
    }

    // Enforce rectilinear (H/V edges)
    void EnforceRectilinear()
    {
        int n = vertexData.quadrantVertexCount;
        if(n < 2)
            return;

        Vector2[] verts = vertexData.GetVertices();

        for(int i = 1; i < n; i++)
        {
            Vector2 prev = verts[i - 1];
            Vector2 curr = verts[i];
            Vector2 delta = curr - prev;

            if(Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                curr.y = prev.y;  // Horizontal
            else
                curr.x = prev.x;  // Vertical

            vertexData.SetVertex(i, curr);
        }
    }

    // Symmetry: first bottom-edge distance from bottom-left
    // equals last right-edge distance from top-right
    void EnforceSymmetryConstraint()
    {
        int n = vertexData.quadrantVertexCount;
        if(n < 2)
            return;

        Vector2[] verts = vertexData.GetVertices();

        // v0.x is in [-Size, 0], clamp just in case
        float x0 = Mathf.Clamp(verts[0].x, -Size, 0f);

        // First: on bottom edge
        Vector2 first = new Vector2(x0, 0f);

        // Last: on right edge, y = -x0 (so distances match)
        float lastY = Mathf.Clamp(-x0, 0f, Size);
        Vector2 last = new Vector2(0f, lastY);

        vertexData.SetVertex(0, first);
        vertexData.SetVertex(n - 1, last);
    }

    void BuildFourFoldPolygon()
    {
        fullVertices.Clear();
        int n = vertexData.quadrantVertexCount;

        AddArm(0f);                          // TL
        AddArm(-90f * Mathf.Deg2Rad);        // TR
        AddArm(180f * Mathf.Deg2Rad);        // BR
        AddArm(-270f * Mathf.Deg2Rad);       // BL

        if(closePolygon && fullVertices.Count > 0)
            fullVertices.Add(fullVertices[0]);
    }

    void AddArm(float angleRad)
    {
        int n = vertexData.quadrantVertexCount;
        float c = Mathf.Cos(angleRad);
        float s = Mathf.Sin(angleRad);

        Vector2[] verts = vertexData.GetVertices();

        for(int i = 0; i < n; i++)
        {
            Vector2 p = verts[i];
            float rx = p.x * c - p.y * s;
            float ry = p.x * s + p.y * c;
            fullVertices.Add(new Vector3(rx, ry, 0f));
        }
    }

    void ResizeQ1Vertices(int targetCount)
    {
        vertexData.quadrantVertexCount = Mathf.Clamp(targetCount, 2, MaxQ1Vertices);

        int n = vertexData.quadrantVertexCount;
        float width = Size;
        float perimeter = 4f * width;
        float step = perimeter / (n - 1);

        for(int i = 0; i < n; i++)
        {
            float dist = i * step;
            Vector2 pos;

            if(dist < width)
                pos = new Vector2(-width + dist, 0f);                      // Bottom
            else if(dist < 2 * width)
                pos = new Vector2(-width, dist - width);                   // Left
            else if(dist < 3 * width)
                pos = new Vector2(-width + (dist - 2 * width), width);     // Top
            else
                pos = new Vector2(0f, 3 * width - dist);                   // Right

            vertexData.SetVertex(i, pos);
        }

        EnforceSymmetryConstraint();
        Debug.Log($"Resized to {n} vertices");
    }

    void Awake()
    {
        if(vertexData.quadrantVertexCount == 0)
            InitDefaultQ1();
        Rebuild();
    }

    void OnValidate()
    {
        if(vertexData == null)
            return;

        int oldCount = vertexData.quadrantVertexCount;
        vertexData.quadrantVertexCount = Mathf.Clamp(vertexData.quadrantVertexCount, 2, MaxQ1Vertices);
        //size = Mathf.Max(0.01f, size);
        size = Mathf.Clamp(size, 0.01f, 200f);

        bool changed = false;

        if(oldCount != vertexData.quadrantVertexCount)
        {
            ResizeQ1Vertices(vertexData.quadrantVertexCount);
            lastVertexCount = vertexData.quadrantVertexCount;
            changed = true;
        }

        if(!Mathf.Approximately(lastSize, size))
        {
            lastSize = size;
            changed = true;
        }

        if(changed || fullVertices.Count == 0)
            Rebuild();
    }

    void OnEnable()
    {
        if(vertexData?.quadrantVertexCount > 0)
            Rebuild();
    }

    void LateUpdate()
    {
        if(Application.isPlaying)
        {
            Rebuild();
        }
    }

    public void SaveCurrentVerticesToExportData()
    {
        if(exportData == null)
        {
            Debug.LogWarning("No PolygonVertexData assigned for export.");
            return;
        }

        if(fullVertices == null || fullVertices.Count == 0)
        {
            Debug.LogWarning("No fullVertices to export.");
            return;
        }

        var t = transform;
        Vector3[] world = new Vector3[fullVertices.Count];
        for(int i = 0; i < fullVertices.Count; i++)
            world[i] = t.TransformPoint(fullVertices[i]);

        //Store Q1 vertex count
        exportData.q1VertexCount = vertexData.quadrantVertexCount;

        exportData.worldVertices = world;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(exportData);
#endif
        Debug.Log($"Exported {world.Length} vertices to PolygonVertexData.");
    }

}

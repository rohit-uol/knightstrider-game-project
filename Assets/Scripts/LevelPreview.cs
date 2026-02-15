using UnityEditor;
using UnityEngine;
using System.Collections;

public class LevelPreview : MonoBehaviour
{
    [Header("Data")]
    public PolygonVertexData polygonData;
    public int q1VertexCount;  // can be read from polygonData.q1VertexCount

    [Header("Rendering")]
    public LineRenderer fullPolygonRenderer; // optional: the static final polygon
    public Material lineMaterial;

    [Header("Appearance")]
    public float segmentWidth = 0.05f;

    [Header("Timing")]
    public float fadeInDuration = 0.5f;
    public float moveDuration = 0.75f;
    public float pauseBetweenSteps = 0.2f;
    public float fadeOutDelay = 0.5f;     // time to wait after rotations finish
    public float fadeOutDuration = 0.75f; // how long the fade-out lasts

    [Header("Rotation Durations")]
    public float q2RotateDuration = 0.75f;
    public float q3RotateDuration = 0.75f;
    public float q4RotateDuration = 0.75f;


    [Header("Offsets)")]
    public Vector3 offsetQ1 = Vector3.zero;
    public Vector3 offsetQ2 = new Vector3(4f, 0f, 0f);    // Q1 -> Q2
    public Vector3 offsetQ3 = new Vector3(4f, -4f, 0f);   // Q2 -> Q3  
    public Vector3 offsetQ4 = new Vector3(0f, -4f, 0f);    // Q3 -> Q4

    LineRenderer segQ1, segQ2, segQ3, segQ4;
    Vector3[] q1Segment;        // Q1 vertices (original order)
    Vector3[] q1SegmentReversed; // Reversed for Q2/Q3/Q4 (left-to-right slide)

    void Awake()
    {
        if(polygonData == null)
            return;

        // Get the current Z of this object
        float targetZ = transform.position.z;
        q1VertexCount = polygonData.q1VertexCount;
        q1Segment = new Vector3[q1VertexCount];

        System.Array.Copy(polygonData.worldVertices, q1Segment, q1VertexCount);

        // 2. Force every vertex to match the current object's Z
        for (int i = 0; i < q1VertexCount; i++)
        {
            q1Segment[i].z = targetZ;
        }

        // Reversed copy for Q2/Q3/Q4 so they slide visually left-to-right
        q1SegmentReversed = new Vector3[q1VertexCount];
        for(int i = 0; i < q1VertexCount; i++)
            q1SegmentReversed[i] = q1Segment[q1VertexCount - 1 - i];

        // Optional: full polygon display
        if(fullPolygonRenderer != null)
        {
            fullPolygonRenderer.positionCount = polygonData.worldVertices.Length;
            fullPolygonRenderer.SetPositions(polygonData.worldVertices);
        }

        // Create four segment renderers
        segQ1 = CreateSegmentRenderer("SegQ1");
        segQ2 = CreateSegmentRenderer("SegQ2");
        segQ3 = CreateSegmentRenderer("SegQ3");
        segQ4 = CreateSegmentRenderer("SegQ4");

        // Q1: original order
        InitSegment(segQ1, q1Segment, offsetQ1);

        // Q2/Q3/Q4: reversed order (left-to-right visual)
        InitSegment(segQ2, q1SegmentReversed, offsetQ1);
        InitSegment(segQ3, q1SegmentReversed, offsetQ1);
        InitSegment(segQ4, q1SegmentReversed, offsetQ1);

        // Start invisible
        SetLineAlpha(segQ1, 0f);
        SetLineAlpha(segQ2, 0f);
        SetLineAlpha(segQ3, 0f);
        SetLineAlpha(segQ4, 0f);
    }

    void OnEnable()
    {
        if(polygonData == null)
            return;
        StartCoroutine(PlayPreview());
    }

    void InitSegment(LineRenderer lr, Vector3[] baseVerts, Vector3 offset)
    {
        int count = baseVerts.Length;
        Vector3[] withOffset = new Vector3[count];
        for(int i = 0; i < count; i++)
            withOffset[i] = baseVerts[i] + offset;

        lr.positionCount = count;
        lr.SetPositions(withOffset);
    }

    void SetLineAlpha(LineRenderer lr, float alpha)
    {
        Color start = lr.startColor;
        Color end = lr.endColor;
        start.a = alpha;
        end.a = alpha;
        lr.startColor = start;
        lr.endColor = end;
    }

    IEnumerator PlayPreview()
    {
        // Fade Q1 in place (original order)
        yield return FadeLine(segQ1, 0f, 1f, fadeInDuration);

        yield return new WaitForSeconds(pauseBetweenSteps);

        // Copy slides Q1 -> Q2 (reversed verts = left-to-right)
        InitSegment(segQ2, q1SegmentReversed, offsetQ1);
        SetLineAlpha(segQ2, 1f);
        yield return SlideSegment(segQ2, q1SegmentReversed, offsetQ1, offsetQ2, moveDuration);

        yield return new WaitForSeconds(pauseBetweenSteps);

        // Copy slides Q2 -> Q3 (reversed verts)
        InitSegment(segQ3, q1SegmentReversed, offsetQ2);
        SetLineAlpha(segQ3, 1f);
        yield return SlideSegment(segQ3, q1SegmentReversed, offsetQ2, offsetQ3, moveDuration);

        yield return new WaitForSeconds(pauseBetweenSteps);

        // Copy slides Q3 -> Q4 (reversed verts)
        InitSegment(segQ4, q1SegmentReversed, offsetQ3);
        SetLineAlpha(segQ4, 1f);
        yield return SlideSegment(segQ4, q1SegmentReversed, offsetQ3, offsetQ4, moveDuration);

        yield return new WaitForSeconds(pauseBetweenSteps);

        // Discrete 90 deg clockwise rotations (no flipping)        
        Vector3[] q2Pos = GetSegmentPositions(q1SegmentReversed, offsetQ2);
        Vector3[] q3Pos = GetSegmentPositions(q1SegmentReversed, offsetQ3);
        Vector3[] q4Pos = GetSegmentPositions(q1SegmentReversed, offsetQ4);


        // Try exactly 1.5 if the shift is purely vertical
        Vector3 pivotOffset = new Vector3(0, -1.5f, 0);

        Vector3 q2Center = GetBoundsCenter(q2Pos) + pivotOffset;
        Vector3 q3Center = GetBoundsCenter(q3Pos) + pivotOffset;
        Vector3 q4Center = GetBoundsCenter(q4Pos) + pivotOffset;

        // Q2: 90 deg clockwise = 3 × 30°
        yield return DiscreteRotate(segQ2, q2Pos, q2Center, -90f, q2RotateDuration);
        yield return new WaitForSeconds(pauseBetweenSteps);

        // Q3: 180 deg clockwise = 6 × 30°  
        yield return DiscreteRotate(segQ3, q3Pos, q3Center, -180f, q3RotateDuration);
        yield return new WaitForSeconds(pauseBetweenSteps);

        // Q4: 270 deg clockwise = 9 × 30°
        yield return DiscreteRotate(segQ4, q4Pos, q4Center, -270f, q4RotateDuration);

        // Fade out all segments simultaneously
        yield return new WaitForSeconds(fadeOutDelay);

        yield return FadeAllSegmentsOut(fadeOutDuration);

    }

    Vector3[] GetSegmentPositions(Vector3[] baseVerts, Vector3 offset)
    {
        int count = baseVerts.Length;
        Vector3[] result = new Vector3[count];
        for(int i = 0; i < count; i++)
            result[i] = baseVerts[i] + offset;
        return result;
    }

    IEnumerator FadeLine(LineRenderer lr, float from, float to, float duration)
    {
        float t = 0f;
        while(t < duration)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, to, t / duration);
            SetLineAlpha(lr, a);
            yield return null;
        }
        SetLineAlpha(lr, to);
    }

    IEnumerator SlideSegment(LineRenderer lr, Vector3[] baseVerts, Vector3 startOffset, Vector3 endOffset, float duration)
    {
        int count = baseVerts.Length;
        Vector3[] current = new Vector3[count];
        float t = 0f;

        while(t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            Vector3 offset = Vector3.Lerp(startOffset, endOffset, u);

            for(int i = 0; i < count; i++)
                current[i] = baseVerts[i] + offset;

            lr.positionCount = count;
            lr.SetPositions(current);
            yield return null;
        }

        // End at final position
        for(int i = 0; i < count; i++)
            current[i] = baseVerts[i] + endOffset;
        lr.positionCount = count;
        lr.SetPositions(current);
    }

    Vector3[] RotateSegmentAroundOrigin(Vector3[] source, float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        Vector3[] result = new Vector3[source.Length];
        for(int i = 0; i < source.Length; i++)
        {
            Vector3 p = source[i];
            float x = p.x * cos - p.y * sin;
            float y = p.x * sin + p.y * cos;
            result[i] = new Vector3(x, y, p.z);
        }
        return result;
    }

    IEnumerator RotateSegment(LineRenderer lr, Vector3[] from, Vector3[] to, float duration)
    {
        int count = from.Length;
        Vector3[] current = new Vector3[count];
        float t = 0f;

        while(t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);
            for(int i = 0; i < count; i++)
                current[i] = Vector3.Lerp(from[i], to[i], u);

            lr.positionCount = count;
            lr.SetPositions(current);
            yield return null;
        }

        lr.positionCount = count;
        lr.SetPositions(to);
    }

    LineRenderer CreateSegmentRenderer(string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.material = lineMaterial;

        // 2. FORCE SORTING (This is the most important part for 2D)
        // "Default" is the standard layer; 100 ensures it's above most sprites.
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 100;

        lr.startWidth = segmentWidth;
        lr.endWidth = segmentWidth;

        // Material handles color, LineRenderer only handles ALPHA
        lr.startColor = Color.white;  // (1,1,1,1) → material tints it red
        lr.endColor = Color.white;  // (1,1,1,1) → material tints it red

        lr.numCapVertices = 2;
        lr.numCornerVertices = 2;

        lr.alignment = LineAlignment.TransformZ; // Fixes the "View" alignment issue
lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
lr.receiveShadows = false;

        return lr;
    }



    Vector3[] RotateSegmentAroundCenter(Vector3[] source, float angleDegrees, Vector3 center)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        Vector3[] result = new Vector3[source.Length];
        for(int i = 0; i < source.Length; i++)
        {
            Vector3 p = source[i] - center;  // Move to origin
            float x = p.x * cos - p.y * sin;
            float y = p.x * sin + p.y * cos;
            result[i] = new Vector3(x, y, p.z) + center;  // Move back
        }
        return result;
    }

    //helper function
    Vector3 GetBoundsCenter(Vector3[] verts)
    {
        Bounds b = new Bounds(verts[0], Vector3.zero);
        for(int i = 1; i < verts.Length; i++)
            b.Encapsulate(verts[i]);
        return b.center;
    }

    IEnumerator DiscreteRotate(LineRenderer lr, Vector3[] from, Vector3 center, float totalDegrees, float duration)
    {
        float stepAngle = -30f; // 30° clockwise steps
        int steps = Mathf.RoundToInt(Mathf.Abs(totalDegrees) / 30f);
        float stepDuration = duration / steps;

        Vector3[] current = from;
        lr.SetPositions(current);

        for(int i = 0; i < steps; i++)
        {
            Vector3[] next = RotateSegmentAroundCenter(current, stepAngle, center);

            float t = 0f;
            while(t < stepDuration)
            {
                t += Time.deltaTime;
                float u = Mathf.Clamp01(t / stepDuration);

                // Interpolate smoothly between current and next 30° step
                Vector3[] intermediate = new Vector3[current.Length];
                for(int j = 0; j < current.Length; j++)
                    intermediate[j] = Vector3.Lerp(current[j], next[j], u);

                lr.positionCount = intermediate.Length;
                lr.SetPositions(intermediate);
                yield return null;
            }

            current = next; // Snap to next 30° position
            lr.SetPositions(current);
        }
    }

    IEnumerator FadeAllSegmentsOut(float duration)
    {
        float t = 0f;

        // Capture initial alphas (in case they differ)
        float aQ1 = segQ1 != null ? segQ1.startColor.a : 0f;
        float aQ2 = segQ2 != null ? segQ2.startColor.a : 0f;
        float aQ3 = segQ3 != null ? segQ3.startColor.a : 0f;
        float aQ4 = segQ4 != null ? segQ4.startColor.a : 0f;

        while(t < duration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / duration);

            if(segQ1 != null)
                SetLineAlpha(segQ1, Mathf.Lerp(aQ1, 0f, u));
            if(segQ2 != null)
                SetLineAlpha(segQ2, Mathf.Lerp(aQ2, 0f, u));
            if(segQ3 != null)
                SetLineAlpha(segQ3, Mathf.Lerp(aQ3, 0f, u));
            if(segQ4 != null)
                SetLineAlpha(segQ4, Mathf.Lerp(aQ4, 0f, u));

            yield return null;
        }

        if(segQ1 != null)
            SetLineAlpha(segQ1, 0f);
        if(segQ2 != null)
            SetLineAlpha(segQ2, 0f);
        if(segQ3 != null)
            SetLineAlpha(segQ3, 0f);
        if(segQ4 != null)
            SetLineAlpha(segQ4, 0f);
    }


}

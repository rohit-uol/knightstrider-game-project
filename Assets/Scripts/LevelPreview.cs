using UnityEditor;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using TMPro;


public class LevelPreview : MonoBehaviour
{
    [Header("Intro Text")]
    public GameObject introTextPrefab;  //  3D object TextMeshPro prefab - NOT UI TextMeshPro!!!
    [HideInInspector] public TextMeshPro introTextTMP;
    [HideInInspector] public Transform introTextInstance;
    public Vector3 introTextOffset = Vector3.zero;  // e.g., (0,2,0)
    public float introFadeInDuration = 1f;
    public float introHoldDuration = 1.5f;
    public float introFadeOutDuration = 1f;

    [Header("Data")]
    public PolygonVertexData polygonData;
    public int q1VertexCount;  // can be read from polygonData.q1VertexCount

    [Header("Rendering")]
    public LineRenderer fullPolygonRenderer; // optional: not used
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


    [Header("Tracer Prefab")]
    public GameObject tracerPrefab;
    public float tracerSpeed = 2f;
    [HideInInspector] public Transform tracerInstance;
    [HideInInspector] public SpriteRenderer tracerSR;

    [Header("Background")]
    public GameObject backgroundPrefab;
    [HideInInspector] public Transform backgroundInstance;
    public Vector3 backgroundOffset = Vector3.zero;

    Coroutine previewRoutine;
    bool isEndingEarly = false;


    void Awake()
    {
        if(polygonData == null)
            return;

        // Get the current Z of this object
        float targetZ = transform.position.z;
        q1VertexCount = polygonData.q1VertexCount;
        q1Segment = new Vector3[q1VertexCount];

        System.Array.Copy(polygonData.worldVertices, q1Segment, q1VertexCount);

        // Force every vertex to match the current object's Z
        for(int i = 0; i < q1VertexCount; i++)
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

        // --- Background spawn (behind everything) ---
        backgroundInstance = null;
        if(backgroundPrefab != null)
        {
            // Instantiate but keeps prefab's local pos
            GameObject bgInst = Instantiate(backgroundPrefab, transform, false);  // false = worldPosStays=false
            backgroundInstance = bgInst.transform;

            SpriteRenderer bgSR = bgInst.GetComponent<SpriteRenderer>();
            if(bgSR != null)
            {
                bgSR.sortingLayerName = "Default";
                bgSR.sortingOrder = 50;
                bgSR.enabled = true;

                // ONLY override Z (world space), keep prefab's local X/Y/scale/rot
                backgroundInstance.position = new Vector3(
                    backgroundInstance.position.x,
                    backgroundInstance.position.y,
                    targetZ
                );

                Debug.Log($"Background spawned at world pos: {backgroundInstance.position} (prefab local: {backgroundInstance.localPosition})");
            }
        }
        else
        {
            Debug.LogWarning("LevelPreview: backgroundPrefab not assigned.");
        }

        // intro Text spawn 
        introTextInstance = null;
        introTextTMP = null;
        if(introTextPrefab != null)
        {
            GameObject textInst = Instantiate(introTextPrefab, transform, false);
            introTextInstance = textInst.transform;
            introTextTMP = textInst.GetComponent<TextMeshPro>();

            if(introTextTMP != null)
            {
       
                introTextInstance.position = new Vector3(
                    introTextInstance.position.x + introTextOffset.x,
                    introTextInstance.position.y + introTextOffset.y,
                    //targetZ + introTextOffset.z
                    targetZ  - 1f
                );
                introTextInstance.localScale = new Vector3(2f, 2f, 1f);  // Medium


                // FORCE alpha=0 start + URP material
                introTextTMP.alpha = 0f;
             
                Debug.Log($"Text: 'Level Preview' pos={introTextInstance.position} scale={introTextInstance.localScale} alpha={introTextTMP.alpha}");
            }
        }


        // --- Tracer prefab spawn ---
        tracerInstance = null;
        tracerSR = null;

        if(tracerPrefab != null)
        {
            GameObject inst = Instantiate(tracerPrefab, transform);
            tracerInstance = inst.transform;
            tracerSR = inst.GetComponent<SpriteRenderer>();

            if(tracerSR != null)
            {
                // FORCE VISIBILITY 
                tracerSR.enabled = false;
                tracerSR.sortingLayerName = "Default";
                tracerSR.sortingOrder = 150;  // Above segments (100)

                // Match segments' Z exactly
                tracerInstance.position = new Vector3(0, 0, targetZ);

                // FORCE SCALE
                tracerInstance.localScale = Vector3.one;

                // FORCE MAGENTA COLOR + OPAQUE for URP visibility test
                tracerSR.color = Color.magenta;  // Bright test color

                // FORCE material as URP Sprite/Lit/Default - if not assigned in inspector
                if(tracerSR.material == null || tracerSR.material.name.Contains("Default"))
                {
                    tracerSR.material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default"));
                }

                Debug.Log("Tracer spawned: pos=" + tracerInstance.position + " scale=" + tracerInstance.localScale + " color=" + tracerSR.color);
            }
            else
            {
                Debug.LogError("LevelPreview: tracerPrefab has NO SpriteRenderer!");
            }
        }

    }

    void OnEnable()
    {
        if(polygonData == null)
            return;
        //StartCoroutine(PlayPreview());
        isEndingEarly = false;
        previewRoutine = StartCoroutine(PlayPreview());
    }

    void Update()
    {
        if(Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("SPACE pressed – ending preview early");
            TryEndPreviewEarly();
        }
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
        // Intro text sequence BEFORE segments
        if(introTextTMP != null)
        {
            yield return FadeTMPAlpha(introTextTMP, 0f, 1f, introFadeInDuration);
            yield return new WaitForSeconds(introHoldDuration);
            yield return FadeTMPAlpha(introTextTMP, 1f, 0f, introFadeOutDuration);
        }

        // Then original Q1 fade
        yield return FadeLine(segQ1, 0f, 1f, fadeInDuration);

        // Fade Q1 in place (original order)
        yield return FadeLine(segQ1, 0f, 1f, fadeInDuration);
        //  if(isEndingEarly)
        //    yield break;

        yield return new WaitForSeconds(pauseBetweenSteps);
        //if(isEndingEarly)
        //  yield break;

        // Copy slides Q1 -> Q2 (reversed verts = left-to-right)
        InitSegment(segQ2, q1SegmentReversed, offsetQ1);
        SetLineAlpha(segQ2, 1f);
        yield return SlideSegment(segQ2, q1SegmentReversed, offsetQ1, offsetQ2, moveDuration);
        // if(isEndingEarly)
        //   yield break;

        yield return new WaitForSeconds(pauseBetweenSteps);
        //if(isEndingEarly)
        //  yield break;

        // Copy slides Q2 -> Q3 (reversed verts)
        InitSegment(segQ3, q1SegmentReversed, offsetQ2);
        SetLineAlpha(segQ3, 1f);
        yield return SlideSegment(segQ3, q1SegmentReversed, offsetQ2, offsetQ3, moveDuration);
        //if(isEndingEarly)
        //  yield break;

        yield return new WaitForSeconds(pauseBetweenSteps);
        //if(isEndingEarly)
        //  yield break;

        // Copy slides Q3 -> Q4 (reversed verts)
        InitSegment(segQ4, q1SegmentReversed, offsetQ3);
        SetLineAlpha(segQ4, 1f);
        yield return SlideSegment(segQ4, q1SegmentReversed, offsetQ3, offsetQ4, moveDuration);
        //if(isEndingEarly)
        //  yield break;

        yield return new WaitForSeconds(pauseBetweenSteps);
        //if(isEndingEarly)
        //  yield break;

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
        // if(isEndingEarly)
        //   yield break;
        yield return new WaitForSeconds(pauseBetweenSteps);

        // Q3: 180 deg clockwise = 6 × 30°  
        yield return DiscreteRotate(segQ3, q3Pos, q3Center, -180f, q3RotateDuration);
        // if(isEndingEarly)
        //   yield break;
        yield return new WaitForSeconds(pauseBetweenSteps);

        // Q4: 270 deg clockwise = 9 × 30°
        yield return DiscreteRotate(segQ4, q4Pos, q4Center, -270f, q4RotateDuration);
        //if(isEndingEarly)
        //  yield break;

        // TRACER PATH (after all rotations)
        yield return StartTracerPath();
        //if(isEndingEarly)
        //  yield break;

        // Fade out all segments simultaneously
        yield return new WaitForSeconds(fadeOutDelay);
        // if(isEndingEarly)
        //   yield break;

        yield return FadeAllSegmentsOut(fadeOutDuration);
        // if(isEndingEarly)
        //   yield break;

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
        lr.enabled = true;

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

    IEnumerator FadeTMPAlpha(TextMeshPro tmp, float from, float to, float duration)
    {
        float t = 0f;
        while(t < duration)
        {
            t += Time.deltaTime;
            tmp.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        tmp.alpha = to;
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

        // FORCE SORTING
        // "Default" standard layer; 
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 100;

        lr.startWidth = segmentWidth;
        lr.endWidth = segmentWidth;

        // LineRenderer to handle ALPHA
        lr.startColor = Color.white;  // (1,1,1,1) 
        lr.endColor = Color.white;  // (1,1,1,1) 

        lr.numCapVertices = 2;
        lr.numCornerVertices = 2;

        lr.alignment = LineAlignment.TransformZ; // Fix "View" alignment bug
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
        float stepAngle = -30f; // 30 degree clockwise steps
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

                // Interpolate smoothly between current and next 30 degree step
                Vector3[] intermediate = new Vector3[current.Length];
                for(int j = 0; j < current.Length; j++)
                    intermediate[j] = Vector3.Lerp(current[j], next[j], u);

                lr.positionCount = intermediate.Length;
                lr.SetPositions(intermediate);
                yield return null;
            }

            current = next; // Snap to next 30 degree position
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

        // get tracer fade (if visible)
        float startTracerAlpha = tracerSR != null ? tracerSR.color.a : 0f;

        // get background alpha
        float aBg = 1f;
        SpriteRenderer bgSR = backgroundInstance != null ? backgroundInstance.GetComponent<SpriteRenderer>() : null;
        if(bgSR != null && bgSR.enabled)
            aBg = bgSR.color.a;


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

            if(tracerSR != null && tracerSR.enabled)
            {
                Color c = tracerSR.color;
                c.a = Mathf.Lerp(startTracerAlpha, 0f, u);
                tracerSR.color = c;
            }

            // ADD: Background fade (same timing)
            if(bgSR != null && bgSR.enabled)
            {
                Color c = bgSR.color;
                c.a = Mathf.Lerp(aBg, 0f, u);
                bgSR.color = c;
            }

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

        if(tracerSR != null)
            tracerSR.enabled = false;

        if(bgSR != null)
            bgSR.color = new Color(bgSR.color.r, bgSR.color.g, bgSR.color.b, 0f);

        // Hide intro text
        if(introTextTMP != null)
        {
            introTextTMP.alpha = 0f;
        }


        // Hide preview in scene
        yield return null;  // final frame
        gameObject.SetActive(false);  // disable render/Update calls

    }

    IEnumerator StartTracerPath()
    {
        Debug.Log("StartTracerPath ENTERED");

        if(tracerSR == null || tracerInstance == null)
        {
            Debug.LogError("StartTracerPath: tracerSR/tracerInstance NULL!");
            yield break;
        }

        // FINAL VISIBILITY CHECKS BEFORE TRACE
        tracerSR.enabled = true;
        tracerInstance.localScale = Vector3.one;  // Re-force scale
        tracerSR.color = Color.magenta;           // Test color
        Debug.Log($"Tracer VIS CHECK: enabled={tracerSR.enabled} pos={tracerInstance.position} scale={tracerInstance.localScale} color={tracerSR.color}");

        Vector3[] rawPath = polygonData.worldVertices;
        Debug.Log($"Tracer path length: {rawPath.Length}");

        // Filter path 
        System.Collections.Generic.List<Vector3> validPath = new System.Collections.Generic.List<Vector3>();
        float targetZ = transform.position.z;  // Match segments
        for(int i = 0; i < rawPath.Length; i++)
        {
            Vector3 v = rawPath[i];
            if(v == Vector3.zero)
                continue;
            v.z = targetZ;  // FORCE Z MATCH - for 2D ortho camera
            if(validPath.Count == 0 || Vector3.Distance(validPath[validPath.Count - 1], v) > 0.01f)
                validPath.Add(v);
        }

        Vector3[] path = validPath.ToArray();
        if(path.Length < 2)
        {
            Debug.LogError("Not enough path vertices.");
            tracerSR.enabled = false;
            yield break;
        }

        // Trace segments - FORCE Z/scale each frame
        for(int i = 0; i < path.Length - 1; i++)
        {
            Vector3 startPos = path[i];
            Vector3 endPos = path[i + 1];
            float distance = Vector3.Distance(startPos, endPos);
            if(distance < 0.01f)
                continue;

            float duration = distance / tracerSpeed;
            Debug.Log($"Tracing {i}: {startPos} → {endPos} (dist={distance:F2}s)");

            float elapsed = 0f;
            while(elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                tracerInstance.position = Vector3.Lerp(startPos, endPos, t);
                tracerInstance.localScale = Vector3.one;  // Anti-scale-loss

                // Orient forward
                Vector3 dir = (endPos - startPos).normalized;
                //float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                //tracerInstance.rotation = Quaternion.Euler(0f, 0f, angle);

                yield return null;
            }
        }

        // Hide after trace (fade integrated below)
        tracerSR.enabled = false;
        Debug.Log("Tracer path COMPLETE");
    }

    void TryEndPreviewEarly()
    {
        if(isEndingEarly)
            return; // already triggered once

        isEndingEarly = true;

        // stop the main preview coroutine if running
        if(previewRoutine != null)
        {
            StopCoroutine(previewRoutine);
            previewRoutine = null;
        }

        // immediately start fade-out
        StartCoroutine(FadeAllSegmentsOut(fadeOutDuration));
    }


}
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;


public class LevelPreview : MonoBehaviour
{
    [Header("Persistent Text")]
    public GameObject persistentTextPrefab;   // 3D TextMeshPro prefab
    //public string persistentText = "Level Preview"; 
    public Vector3 persistentTextLocalPos = new Vector3(0f, 0f, -1f);
    public Vector3 persistentTextLocalScale = new Vector3(2f, 4f, 1f);

    Transform persistentTextInstance;
    TMPro.TextMeshPro persistentTMP;

    [Header("Introduction Text")]
    public GameObject introTextPrefab;  //  3D object TextMeshPro prefab - NOT UI TextMeshPro!!!
    [HideInInspector] public TextMeshPro introTextTMP;
    [HideInInspector] public Transform introTextInstance;
    public Vector3 introTextOffset = new Vector3(0f, 2f, 0f);
    public float introFadeInDuration = 1f;
    public float introHoldDuration = 1.5f;
    public float introFadeOutDuration = 1f;

    [Header("Instruction Text")]
    public string tracerInstructionText = "Buttons WASD to move the Knight";
    public Vector3 tracerInstructionTextOffset = new Vector3(0f, -6f, -1f);
    public float instructionFadeIn = 0.4f;
    public float instructionHold = 1.2f;
    public float instructionFadeOut = 0.4f;

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
    [SerializeField] float tracerRevealFadeDuration = 0.25f; // tune


    [Header("Background")]
    public GameObject backgroundPrefab;
    [HideInInspector] public Transform backgroundInstance;
    public Vector3 backgroundOffset = Vector3.zero;

    public UnityEvent OnEnded;

    Coroutine previewRoutine;
    bool isEndingEarly = false;


    void Awake()
    {
        if(polygonData == null)
            return;

        if(persistentTextPrefab != null && persistentTextInstance == null)
        {
            GameObject go = Instantiate(persistentTextPrefab, transform);
            persistentTextInstance = go.transform;
            persistentTextInstance.localPosition = persistentTextLocalPos;
            persistentTextInstance.localScale = persistentTextLocalScale;

            persistentTMP = go.GetComponent<TMPro.TextMeshPro>();
            if(persistentTMP != null)
            {
                //persistentTMP.text = persistentText;
                // Ensure immediately visible
                Color c = persistentTMP.color;
                c.a = 1f;
                persistentTMP.color = c;
            }
        }

        // Get the current Z of this object
        float targetZ = transform.position.z;
        q1VertexCount = polygonData.q1VertexCount;
        q1Segment = new Vector3[q1VertexCount];

        System.Array.Copy(polygonData.worldVertices, q1Segment, q1VertexCount);

        // Force every vertex to match current object's Z
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
                    targetZ - 1f
                );
                introTextInstance.localScale = new Vector3(2f, 2f, 1f);  // Medium


                // FORCE alpha=0 start + URP material
                introTextTMP.alpha = 0f;

                Debug.Log($"Text: 'Level Preview' pos={introTextInstance.position} scale={introTextInstance.localScale} alpha={introTextTMP.alpha}");
            }
        }


        // Tracer prefab spawn ---
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

                // DEBUGGING: FORCE MAGENTA COLOR + OPAQUE for URP visibility test
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

        // Fade Q1 in place (original order)
        yield return FadeLine(segQ1, 0f, 1f, fadeInDuration);

        yield return new WaitForSeconds(pauseBetweenSteps);

        // Try exactly 1.5 if the shift is purely vertical
        Vector3 pivotOffset = new Vector3(0, -1.5f, 0);


        // 1. Q2: slide from Q1 → rotate 90 degs right
        InitSegment(segQ2, q1SegmentReversed, offsetQ1);
        SetLineAlpha(segQ2, 1f);
        yield return SlideSegment(segQ2, q1SegmentReversed, offsetQ1, offsetQ2, moveDuration);
        yield return new WaitForSeconds(pauseBetweenSteps);
        Vector3[] q2Pos = new Vector3[segQ2.positionCount];
        segQ2.GetPositions(q2Pos);
        Vector3 q2Center = GetBoundsCenter(q2Pos) + pivotOffset;
        yield return DiscreteRotate(segQ2, q2Pos, q2Center, -90f, q2RotateDuration);

        yield return new WaitForSeconds(pauseBetweenSteps);

        // 1) Copy Q2 final world positions (already rotated, already sitting at Q2)
        Vector3[] q2Rotated = new Vector3[segQ2.positionCount];
        segQ2.GetPositions(q2Rotated);

        // 2) Put Q3 exactly on top of Q2 (NO extra offset!)
        InitSegment(segQ3, q2Rotated, Vector3.zero);
        SetLineAlpha(segQ3, 1f);

        // 3) Slide one “square” down
        Vector3 relQ2toQ3 = offsetQ3 - offsetQ2;
        yield return SlideSegment(segQ3, q2Rotated, Vector3.zero, relQ2toQ3, moveDuration);

        yield return new WaitForSeconds(pauseBetweenSteps);

        // 4) Rotate Q3 90 degs clockwise around
        Vector3[] q3Current = new Vector3[segQ3.positionCount];
        segQ3.GetPositions(q3Current);

        // Diagonal midpoint (first node -> last node)
        Vector3 q3Center = (q3Current[0] + q3Current[q3Current.Length - 1]) * 0.5f;

        yield return DiscreteRotate(segQ3, q3Current, q3Center, -90f, q3RotateDuration);
        yield return new WaitForSeconds(pauseBetweenSteps);

        // 3. Q4: copy rotated Q3 → slide left → rotate 90 degs right

        // 1) Copy Q3 final world positions (already rotated)
        Vector3[] q3Rotated = new Vector3[segQ3.positionCount];
        segQ3.GetPositions(q3Rotated);

        // 2) Put Q4 exactly on top of Q3 (NO extra offset!)
        InitSegment(segQ4, q3Rotated, Vector3.zero);
        SetLineAlpha(segQ4, 1f);

        // 3) Slide one “square” left: use RELATIVE offsets (0 -> offsetQ4-offsetQ3)
        Vector3 relQ3toQ4 = offsetQ4 - offsetQ3;
        yield return SlideSegment(segQ4, q3Rotated, Vector3.zero, relQ3toQ4, moveDuration);

        yield return new WaitForSeconds(pauseBetweenSteps);

        // 4) Rotate Q4 90 degs clockwise around diagonal midpoint (first <-> last)
        Vector3[] q4Current = new Vector3[segQ4.positionCount];
        segQ4.GetPositions(q4Current);

        Vector3 q4Center = (q4Current[0] + q4Current[q4Current.Length - 1]) * 0.5f;
        // (Don’t add pivotOffset here; only add it if you *prove* you still need it.)

        yield return DiscreteRotate(segQ4, q4Current, q4Center, -90f, q4RotateDuration);

        yield return new WaitForSeconds(pauseBetweenSteps);



        yield return new WaitForSeconds(pauseBetweenSteps);


        // TRACER PATH (after all rotations)
        yield return StartTracerPath();

        // Fade out all segments simultaneously
        yield return new WaitForSeconds(fadeOutDelay);

        yield return FadeAllSegmentsOut(fadeOutDuration);

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

        // Hide persistent text
        if(persistentTextInstance != null)
            persistentTextInstance.gameObject.SetActive(false);


        // Hide preview in scene
        yield return null;  // final frame
        OnEnded.Invoke();
        gameObject.SetActive(false);  // disable render/Update calls

    }

    IEnumerator StartTracerPath()
    {
        Debug.Log("StartTracerPath ENTERED");

        if(tracerSR == null || tracerInstance == null)
        {
            Debug.LogError("StartTracerPath: tracerSR/tracerInstance NULL.");
            yield break;
        }

        if(polygonData == null || polygonData.worldVertices == null || polygonData.worldVertices.Length < 2)
        {
            Debug.LogError("StartTracerPath: polygonData/worldVertices missing or too short.");
            tracerSR.enabled = false;
            yield break;
        }

        tracerSR.enabled = true;
        tracerSR.color = new Color(tracerSR.color.r, tracerSR.color.g, tracerSR.color.b, 1f);
        tracerInstance.localScale = Vector3.one;

        Vector3[] path = polygonData.worldVertices;
        int n = path.Length;

        if(n % 4 != 0)
            Debug.LogWarning($"StartTracerPath: path length {n} not divisible by 4; quarter pauses may be off.");

        int quarter = n / 4;

        int q1End = quarter - 1;
        int q2End = (2 * quarter) - 1;
        int q3End = (3 * quarter) - 1;
        int q4End = n - 1;

        int revealQ2At = q2End;
        int revealQ3At = q3End;
        int revealQ4At = q4End;

        float z = transform.position.z;

        Vector3 first = path[0];
        first.z = z;
        tracerInstance.position = first;

        // Hide Q2/Q3/Q4 just before tracer starts moving
        SetLineAlpha(segQ2, 0f);
        SetLineAlpha(segQ3, 0f);
        SetLineAlpha(segQ4, 0f);

        yield return StartCoroutine(ShowInstructionText(
     "Buttons WASD to move the knight",
     tracerInstructionTextOffset,
     instructionFadeIn,
     instructionHold,
     instructionFadeOut
 ));

        // tracer appears and waits before moving
        yield return new WaitForSeconds(pauseBetweenSteps);

        for(int i = 0; i < n - 1; i++)
        {
            Vector3 startPos = path[i];
            Vector3 endPos = path[i + 1];
            startPos.z = z;
            endPos.z = z;

            float distance = Vector3.Distance(startPos, endPos);
            if(distance < 0.01f)
            {
                int arrivedIdxZero = i + 1;

                if(arrivedIdxZero == q1End || arrivedIdxZero == q2End || arrivedIdxZero == q3End)
                    yield return new WaitForSeconds(pauseBetweenSteps);

                continue;
            }

            float duration = distance / Mathf.Max(0.0001f, tracerSpeed);

            float elapsed = 0f;
            while(elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                tracerInstance.position = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            tracerInstance.position = endPos;

            int arrivedIdx = i + 1;

            // Reveal segments when tracer reaches end of Q2/Q3/Q4
            if(arrivedIdx == revealQ2At)
            {
                yield return FadeLine(segQ2, 0f, 1f, tracerRevealFadeDuration);
            }
            else if(arrivedIdx == revealQ3At)
            {
                yield return FadeLine(segQ3, 0f, 1f, tracerRevealFadeDuration);
            }
            else if(arrivedIdx == revealQ4At)
            {
                yield return FadeLine(segQ4, 0f, 1f, tracerRevealFadeDuration);
            }

            // Pause at end of Q1, Q2, Q3, Q4
            if(arrivedIdx == q1End || arrivedIdx == q2End || arrivedIdx == q3End || arrivedIdx == q4End)
                yield return new WaitForSeconds(pauseBetweenSteps);
        }

        // BUG FIX: ensure Q4 is visible even if revealQ4At was skipped
        // (issue was last "edge" was zero-length and got 'continue;'d above)
        if(segQ4 != null)
        {
            Color c = segQ4.startColor;
            if(c.a < 0.99f)
                yield return FadeLine(segQ4, c.a, 1f, tracerRevealFadeDuration);
        }

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

    //called in StartTracerPath function
    IEnumerator ShowInstructionText(string msg, Vector3 offset, float fadeIn, float hold, float fadeOut)
    {
        if(introTextTMP == null)
            yield break;

        introTextTMP.transform.localPosition = offset;
        introTextTMP.text = msg;

        // start invisible
        Color c = introTextTMP.color;
        c.a = 0f;
        introTextTMP.color = c;
        introTextTMP.gameObject.SetActive(true);

        // fade in
        float t = 0f;
        while(t < fadeIn)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0f, 1f, t / fadeIn);
            Color cc = introTextTMP.color;
            cc.a = a;
            introTextTMP.color = cc;
            yield return null;
        }

        // hold
        yield return new WaitForSeconds(hold); // standard coroutine delay 

        // fade out
        t = 0f;
        while(t < fadeOut)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeOut);
            Color cc = introTextTMP.color;
            cc.a = a;
            introTextTMP.color = cc;
            yield return null;
        }

        // optional: keep disabled until needed again
        introTextTMP.gameObject.SetActive(false);
    }


}
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using System;

namespace TheMasterPath
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Movement : MonoBehaviour
    {
        public event Action<Vector2, Vector2> StepStarted;
        public event Action<Vector2, Vector2> StepEnded;
        public event Action<Vector2> TurnBack;

        public bool EnableInput { get; set; } = true;

        [SerializeField]
        Tilemap masterPathTilemap;

        [Header("Hidable Tilemaps")]
        [SerializeField] Tilemap hideableTilemap1;
        [SerializeField] Tilemap hideableTilemap2;
        [SerializeField] Tilemap hideableTilemap3;

        [Header("Transition Settings")]
        [Tooltip("How fast the tilemap fades (higher is faster)")]
        [SerializeField] float fadeSpeed = 5f;
        [Range(0f, 1f)]
        [SerializeField] float hiddenAlpha = 0.2f; // Not fully invisible, but transparent

        [SerializeField]
        Collider2D triggerCollider;

        // Store original alphas here
        private Dictionary<Tilemap, float> originalAlphas = new Dictionary<Tilemap, float>();

        /// <summary>
        /// How long a step takes in seconds.
        /// </summary>
        [SerializeField]
        float stepTime = 0.25f;

        /// <summary>
        /// How far a step goes in meters.
        /// </summary>
        [SerializeField]
        float stepSize = 1f;

        /// <summary>
        /// Curve used for the hop animation during a step.
        /// </summary>
        [SerializeField]
        AnimationCurve stepCurve;

        Rigidbody2D rb;
        Animator anim;

        Vector2 input = Vector2.zero;
        bool isMoving = false;

        /// <summary>
        /// The duration of a step.
        /// </summary>
        float stepTimer = 0f;

        /// <summary>
        /// The initial position of a step.
        /// </summary>
        Vector2 stepStart = Vector2.zero;
        
        /// <summary>
        /// The target position of a step.
        /// </summary>
        Vector2 stepEnd = Vector2.zero;
        
        /// <summary>
        /// Did the player move already? (reset when a key is released)
        /// </summary>
        bool didMove = false;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            // Search children for the Animator component
            anim = GetComponentInChildren<Animator>();

            // Capture the starting alpha values
            CaptureAlpha(hideableTilemap1);
            CaptureAlpha(hideableTilemap2);
            CaptureAlpha(hideableTilemap3);

            if (anim == null)
            {
                Debug.LogWarning("Animator not found on " + gameObject.name + " or its children!");
            }
        }

        void Update()
        {
            // Always update animation state
            UpdateAnimationParameters();

            if (EnableInput)
            {
                CheckInput();
            }

            // Handle the transparency transitions
            // UpdateTilemapFading();
        }

        void FixedUpdate()
        {
            MoveRigidbody();
        }

        /// <summary>
        /// Message called by the PlayerInput component.
        /// </summary>
        void OnMove(InputValue value)
        {
            input = value.Get<Vector2>();

            // don't allow diagonal movement
            if (!Mathf.Approximately(input.y, 0f))
            {
                input.x = 0;
            }
            else if (!Mathf.Approximately(input.x, 0f))
            {
                input.y = 0;
            }
        }

        /// <summary>
        /// Initiates a step that moves the player from one position to another over time.
        /// </summary>
        public void MoveTo(Vector2 position)
        {
            isMoving = true;
            stepStart = rb.position;
            stepEnd = position;
            OnStepStarted();
        }

        public void Teleport(Vector2 position)
        {
            isMoving = false;
            stepTimer = 0f; // Reset the timer

            // Crucial: Update these so if CheckPath() runs, 
            // it thinks we are already "at" the destination.
            rb.position = position;
            transform.position = position;
            stepStart = position;
            stepEnd = position;
        }

        /// <summary>
        /// Checks input and initiates movement if possible.
        /// </summary>
        void CheckInput()
        {
            if (input.magnitude > 0f && !isMoving && !didMove)
            {
                MoveTo(rb.position + input.normalized * stepSize);
            }

            if (didMove && input.magnitude <= 0f && !isMoving)
            {
                didMove = false;
            }
        }

        /// <summary>
        /// Moves the rigibody from one point to another over time.
        /// A single move is called a "step".
        /// </summary>
        void MoveRigidbody()
        {
            if (!isMoving) return;

            var curr = stepStart;
            var dest = stepEnd;
            var t = stepTimer / stepTime;
            t = Mathf.Min(t, 1f);
            var move = (dest - curr) * t;

            var hopOffset = GetStepAnimationDirection(move.normalized) * 0.25f * stepCurve.Evaluate(t);
            rb.MovePosition(stepStart + move + hopOffset);
            stepTimer += Time.fixedDeltaTime;

            if (t >= 1f)
            {
                stepTimer = 0f;
                isMoving = false;
                didMove = true;

                OnStepEnded();
            }
        }

        Vector2 GetStepAnimationDirection(Vector2 stepDirection)
        {
            if (stepDirection.x == 1 || stepDirection.x == -1)
            {
                return new Vector2(0, 1);
            }
            else if (stepDirection.y == 1 || stepDirection.y == -1)
            {
                return new Vector2(1, 0);
            }

            return Vector2.zero;
        }

        /// <summary>
        /// Called when the player starts moving.
        /// </summary>
        void OnStepStarted()
        {
            triggerCollider.enabled = false;
            StepStarted?.Invoke(stepStart, stepEnd);
        }

        /// <summary>
        /// Called when the players stops moving.
        /// </summary>
        void OnStepEnded()
        {
            triggerCollider.enabled = true;
            CheckPath();
            StepEnded?.Invoke(stepStart, stepEnd);
        }

        /// <summary>
        /// Checks if the player is on the master path and invokes the TurnBack event if not.
        /// </summary>
        void CheckPath()
        {
            var cell = masterPathTilemap.WorldToCell(stepEnd);
            var tile = masterPathTilemap.GetTile(cell);
            if (tile == null)
            {
                TurnBack?.Invoke(stepStart);
                EnableInput = false;
            }
        }

        void UpdateAnimationParameters()
        {
            if (anim == null) return;
            
            // Logic: Moving if we haven't reached target yet OR if we are pushing a key
            bool isMovingNow = Vector2.Distance(rb.position, stepEnd) > 0.05f || input.magnitude > 0.1f;

            //anim.SetBool("isMoving", isMovingNow);

            if (input.magnitude > 0.1f)
            {
                anim.SetFloat("MoveX", input.x);
                anim.SetFloat("MoveY", input.y);

                // Flip the CHILD object (the sprite) based on X direction
                // We use anim.transform so we only flip the sprite, not the parent logic
                if (input.x < 0) anim.transform.localScale = new Vector3(-1, 1, 1);
                else if (input.x > 0) anim.transform.localScale = new Vector3(1, 1, 1);
            }
        }

        /// <summary>
        /// Manages the smooth alpha transition for all three tilemaps.
        /// </summary>
        void UpdateTilemapFading()
        {
            HandleFade(hideableTilemap1);
            HandleFade(hideableTilemap2);
            HandleFade(hideableTilemap3);
        }

        void HandleFade(Tilemap map)
        {
            if (map == null || !originalAlphas.ContainsKey(map)) return;
            // 1. Get the player's current position in World Space
            Vector3 playerPos = rb.position;

            // 2. Convert that exact world point to the Tilemap's local Grid coordinate
            // Note: If your tiles are small, you might want to use rb.worldCenter 
            // to check the middle of the player instead of their feet.
            Vector3Int cellPos = map.WorldToCell(playerPos);

            // 3. Check if a tile exists at that coordinate
            bool isOverlapping = map.HasTile(cellPos);

            // 4. Determine target alpha
            float targetAlpha = isOverlapping ? hiddenAlpha : originalAlphas[map];

            // 5. Apply the Lerp/MoveTowards
            Color currentColor = map.color;
            if (!Mathf.Approximately(currentColor.a, targetAlpha))
            {
                float newAlpha = Mathf.MoveTowards(currentColor.a, targetAlpha, fadeSpeed * Time.deltaTime);
                map.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
            }
        }

        void CaptureAlpha(Tilemap map)
        {
            if (map != null)
            {
                originalAlphas[map] = map.color.a;
            }
        }

    }

    }
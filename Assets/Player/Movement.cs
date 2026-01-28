using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheMasterPath
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Movement : MonoBehaviour
    {
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

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            // Search children for the Animator component
            anim = GetComponentInChildren<Animator>();

            if (anim == null)
            {
                Debug.LogWarning("Animator not found on " + gameObject.name + " or its children!");
            }
        }

        void Update()
        {
            // Always update animation state
            UpdateAnimationParameters();
            CheckInput();
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
        /// Checks input and starts movement if possible.
        /// </summary>
        void CheckInput()
        {
            if (input.magnitude > 0f && !isMoving)
            {
                isMoving = true;
                stepStart = rb.position;
                stepEnd = rb.position + input.normalized * stepSize;
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

            rb.MovePosition(stepStart + move);
            stepTimer += Time.fixedDeltaTime;

            if (t >= 1f)
            {
                stepTimer = 0f;
                isMoving = false;
            }
        }

        void UpdateAnimationParameters()
        {
            if (anim == null) return;
            
            // Logic: Moving if we haven't reached target yet OR if we are pushing a key
            bool isMovingNow = Vector2.Distance(rb.position, stepEnd) > 0.05f || input.magnitude > 0.1f;

            anim.SetBool("isMoving", isMovingNow);

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
    }
}
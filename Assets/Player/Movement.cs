using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheMasterPath
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Movement : MonoBehaviour
    {
        [SerializeField]
        bool enableGridMovement = false;

        [Header("Free Movement")]
        [SerializeField]
        float speed = 5f;

        [Header("Grid-based Movement")]
        [SerializeField]
        float stepDelay = 0.2f;

        [SerializeField]
        float stepSize = 1f;

        [SerializeField]
        float stepSpeed = 5f;

        Rigidbody2D rb;
        Animator anim;
        Vector2 input;
        float timer;
        Vector2 targetPosition;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();

            // Search children for the Animator component
            anim = GetComponentInChildren<Animator>();

            targetPosition = rb.position;

            if (anim == null)
            {
                Debug.LogWarning("Animator not found on " + gameObject.name + " or its children!");
            }
        }

        void OnMove(InputValue value)
        {
            input = value.Get<Vector2>();

            if (!Mathf.Approximately(input.y, 0f))
            {
                input.x = 0;
            }
            else if (!Mathf.Approximately(input.x, 0f))
            {
                input.y = 0;
            }
        }

        void Update()
        {
            // Always update animation state
            UpdateAnimationParameters();

            if (!enableGridMovement) return;

            if (input.magnitude > 0f)
            {
                timer -= Time.deltaTime;

                if (timer <= 0f)
                {
                    targetPosition = rb.position + input.normalized * stepSize;
                    timer = stepDelay;
                }
            }
            else
            {
                timer = Mathf.Max(0f, timer - Time.deltaTime);
            }
        }

        void UpdateAnimationParameters()
        {
            if (anim == null) return;

            // Logic: Moving if we haven't reached target yet OR if we are pushing a key
            bool isMovingNow = Vector2.Distance(rb.position, targetPosition) > 0.05f || input.magnitude > 0.1f;

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

        void FixedUpdate()
        {
            if (enableGridMovement)
            {
                rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, Time.deltaTime * stepSpeed));
            }
            else
            {
                // In free movement, targetPosition needs to follow the RB so transitions don't break
                rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
                targetPosition = rb.position;
            }
        }
    }
}
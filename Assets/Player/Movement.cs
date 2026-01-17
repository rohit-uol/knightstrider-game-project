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
        float speed;

        [Header("Grid-based Movement")]
        [SerializeField]
        float stepDelay = 0.2f;

        [SerializeField]
        float stepSize = 1f;

        [SerializeField]
        float stepSpeed = 5f;

        Rigidbody2D rb;
        Vector2 input;
        float timer;
        Vector2 targetPosition;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            targetPosition = rb.position;
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

        void FixedUpdate()
        {
            if (enableGridMovement)
            {
                rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, Time.deltaTime * stepSpeed));
            }
            else
            {
                rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
            }
        }
    }
}
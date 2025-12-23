using UnityEngine;
using UnityEngine.InputSystem;

namespace KnightStrider
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Movement : MonoBehaviour
    {
        [SerializeField]
        float speed;

        Rigidbody2D rb;
        Vector2 input;

        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
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

        void FixedUpdate()
        {
            rb.MovePosition(rb.position + input * speed * Time.fixedDeltaTime);
        }
    }
}
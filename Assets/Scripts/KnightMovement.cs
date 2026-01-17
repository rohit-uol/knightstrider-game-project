using UnityEngine;
using UnityEngine.InputSystem; // This is the vital line for Unity 6

public class KnightMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Animator anim;
    private Vector3 targetPosition;
    private bool isMoving;

    void Start()
    {
        anim = GetComponent<Animator>();
        targetPosition = transform.position;
    }

    void Update()
    {
        // Smoothly slide to the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Only look for new input if we are 'at' our target (Snappy Grid Movement)
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            Vector2 input = Vector2.zero;

            // This is the Unity 6 way to check keys
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y = 1;
            else if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y = -1;
            else if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x = -1;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x = 1;

            if (input != Vector2.zero)
            {
                targetPosition += new Vector3(input.x, input.y, 0);
                UpdateAnimation(input.x, input.y);
            }
            else
            {
                anim.SetBool("isMoving", false);
            }
        }
    }

    void UpdateAnimation(float x, float y)
    {
        anim.SetBool("isMoving", true);
        anim.SetFloat("MoveX", x);
        anim.SetFloat("MoveY", y);

        // Flip the knight to face left or right
        if (x < 0) transform.localScale = new Vector3(-1, 1, 1);
        else if (x > 0) transform.localScale = new Vector3(1, 1, 1);
    }
}
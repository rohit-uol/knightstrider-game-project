using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class TilemapFade : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject targetObject; // The object that triggers transparency
    [Range(0f, 1f)]
    [SerializeField] private float transparentAlpha = 0.3f; // How see-through it becomes
    [SerializeField] private float fadeSpeed = 5f; // How fast it fades in/out

    private Tilemap tilemap;
    private float targetAlpha = 1f;
    private Color currentColor;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        currentColor = tilemap.color;
    }

    void Update()
    {
        // Smoothly interpolate the alpha value
        float newAlpha = Mathf.MoveTowards(tilemap.color.a, targetAlpha, fadeSpeed * Time.deltaTime);
        tilemap.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object entering is our target
        if (other.gameObject == targetObject)
        {
            targetAlpha = transparentAlpha;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Reset when the target leaves
        if (other.gameObject == targetObject)
        {
            targetAlpha = 1f;
        }
    }
}
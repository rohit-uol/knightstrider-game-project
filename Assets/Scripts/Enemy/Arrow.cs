using UnityEngine;

namespace TheMasterPath
{
    /// <summary>
    /// Moves in a straight line toward the direction set by EnemyController at spawn time.
    /// Deals one point of damage to the player on contact, then destroys itself.
    /// Requires: Rigidbody2D (Kinematic, Gravity Scale 0) + CircleCollider2D (Is Trigger) on the prefab.
    /// </summary>
    public class Arrow : MonoBehaviour
    {
        // Set by EnemyController immediately after Instantiate
        [HideInInspector] public Vector2 direction;
        [HideInInspector] public float speed = 4f;

        [Tooltip("Seconds before the arrow self-destructs if it doesn't hit anything.")]
        [SerializeField] private float lifetime = 6f;

        private void Start()
        {
            // Auto-destroy so missed arrows don't pile up forever
            Destroy(gameObject, lifetime);

            // Rotate sprite to face travel direction (assumes arrow points right by default)
            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }

        private void Update()
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Ignore the enemy that fired this arrow
            if (other.CompareTag("Enemy")) return;

            // If we hit the player, deal damage
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.TakeDamage();
                Destroy(gameObject);
                return;
            }

            // Destroy on any solid object (walls, tiles, etc.)
            // Only destroy on non-trigger colliders so we don't vanish on zone triggers
            if (!other.isTrigger)
                Destroy(gameObject);
        }
    }
}

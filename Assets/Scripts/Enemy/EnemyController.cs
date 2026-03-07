using System.Collections;
using UnityEngine;
using TheMasterPath.Utilities;

namespace TheMasterPath
{
    /// <summary>
    /// An enemy that becomes active while the player is in a specific map quadrant.
    /// Fires arrows toward the player at a configurable interval.
    ///
    /// Setup in the Inspector:
    ///   1. Set Active Quadrant (1–4, clockwise from top-left).
    ///   2. Assign Arrow Prefab (needs Arrow.cs + Rigidbody2D Kinematic + CircleCollider2D Is Trigger).
    ///   3. Optionally assign a Fire Point child transform (defaults to this object's position).
    ///   4. Tag the Enemy root GameObject as "Enemy" so arrows ignore it.
    /// </summary>
    public class EnemyController : MonoBehaviour
    {
        [Header("Quadrant Activation")]
        [Tooltip("The quadrant number (1–4) this enemy guards. Mirrors NavigationUtils.GetQuadrant().")]
        [SerializeField] private int activeQuadrant = 1;

        [Header("Arrow Firing")]
        [Tooltip("Drag the Arrow prefab here (Arrow.cs, Rigidbody2D Kinematic, CircleCollider2D Is Trigger).")]
        [SerializeField] private GameObject arrowPrefab;
        [Tooltip("Spawn point for arrows. Leave empty to fire from this object's position.")]
        [SerializeField] private Transform firePoint;
        [Tooltip("Seconds between each arrow shot.")]
        [SerializeField] private float fireInterval = 2f;
        [Tooltip("Arrow travel speed. ~3 is slow / easy to dodge; ~7 is fast.")]
        [SerializeField] private float arrowSpeed = 3f;

        // ── internals ──────────────────────────────────────────────
        private Transform   _player;
        private Renderer[]  _renderers;
        private Collider2D  _collider;
        private Coroutine   _fireRoutine;
        private bool        _isActive;

        // ──────────────────────────────────────────────────────────
        private void Start()
        {
            // Cache all renderers so we can show/hide the whole hierarchy
            _renderers = GetComponentsInChildren<Renderer>();
            _collider  = GetComponent<Collider2D>();

            // Find the player (Health is on the player root)
            Health playerHealth = FindObjectOfType<Health>();
            if (playerHealth != null)
                _player = playerHealth.transform;
            else
                Debug.LogWarning($"[EnemyController] Could not find a Health component in the scene.");

            // Start hidden; Update will enable when player enters the quadrant
            SetVisible(false);
        }

        private void Update()
        {
            if (_player == null) return;

            int playerQuadrant = NavigationUtils.GetQuadrant(_player.position);
            bool shouldBeActive = (playerQuadrant == activeQuadrant);

            if (shouldBeActive && !_isActive)
                Activate();
            else if (!shouldBeActive && _isActive)
                Deactivate();
        }

        // ──────────────────────────────────────────────────────────
        private void Activate()
        {
            _isActive = true;
            SetVisible(true);
            if (_fireRoutine == null)
                _fireRoutine = StartCoroutine(FireLoop());
        }

        private void Deactivate()
        {
            _isActive = false;
            SetVisible(false);
            if (_fireRoutine != null)
            {
                StopCoroutine(_fireRoutine);
                _fireRoutine = null;
            }
        }

        private void SetVisible(bool visible)
        {
            foreach (Renderer r in _renderers)
                r.enabled = visible;

            if (_collider != null)
                _collider.enabled = visible;
        }

        // ──────────────────────────────────────────────────────────
        /// <summary>Fires an arrow toward the player every fireInterval seconds.</summary>
        private IEnumerator FireLoop()
        {
            // Small initial delay so the enemy doesn't fire the instant it activates
            yield return new WaitForSeconds(fireInterval * 0.5f);

            while (true)
            {
                FireArrow();
                yield return new WaitForSeconds(fireInterval);
            }
        }

        private void FireArrow()
        {
            if (arrowPrefab == null || _player == null) return;

            // Prefer the assigned firePoint; fall back to the sprite's visual centre
            // (renderer bounds) rather than the root transform which may be offset.
            Vector3 origin;
            if (firePoint != null)
            {
                origin = firePoint.position;
            }
            else
            {
                Renderer rend = GetComponentInChildren<Renderer>();
                origin = rend != null ? rend.bounds.center : transform.position;
            }

            Vector2 dir    = (_player.position - origin).normalized;

            GameObject arrowGO = Instantiate(arrowPrefab, origin, Quaternion.identity);

            Arrow arrow = arrowGO.GetComponent<Arrow>();
            if (arrow != null)
            {
                arrow.direction = dir;
                arrow.speed     = arrowSpeed;
            }
            else
            {
                Debug.LogWarning("[EnemyController] Arrow prefab is missing the Arrow script.");
            }
        }

        // ──────────────────────────────────────────────────────────
        /// <summary>Draw a wire sphere in the editor so you can see the fire point.</summary>
        private void OnDrawGizmosSelected()
        {
            if (firePoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(firePoint.position, 0.15f);
            }
        }
    }
}

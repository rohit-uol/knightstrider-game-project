using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

namespace TheMasterPath.Utilities
{
    public class MapDestroyer : MonoBehaviour
    {

        public static MapDestroyer Instance; // Accessible from anywhere

    

        [Header("Targeting")]
        [SerializeField] private Tilemap[] targetTilemaps;
        [Tooltip("Drag the Props parent Transform here (Map/Layer 1/Props).")]
        [SerializeField] private Transform propsParent;
        [Tooltip("Drag individual buildings or large objects here (e.g. Building Custom1). Destroyed per-quadrant.")]
        [SerializeField] private GameObject[] trackedObjects;

        [Header("Dissolve Effect")]
        [Tooltip("Ghost-tile prefab: an empty GameObject with a SpriteRenderer using Mat_Dissolve.")]
        [SerializeField] private GameObject dissolvePrefab;
        [Tooltip("Higher value = faster dissolve. 2 ≈ 0.5 s, 1 ≈ 1 s.")]
        [SerializeField] private float dissolveSpeed = 2f;
        [Tooltip("AudioSource on this GameObject used to play the dissolve SFX.")]
        [SerializeField] private AudioSource audioSource;
        [Tooltip("Assign Assets/SFX/Shatter/freesound_community-rock-smash-6304.mp3 or any clip.")]
        [SerializeField] private AudioClip dissolveClip;
        [Tooltip("_DissolveAmount value where the ghost tile stops (0 = solid, 1 = fully gone). Default 0.85 leaves a crumbled look instead of revealing black.")]
        [SerializeField, Range(0f, 1f)] private float dissolveStopAt = 0.85f;
        [Tooltip("How fast the edge breathes after the dissolve completes (keeps fire flicker alive).")]
        [SerializeField] private float edgeBreathSpeed = 4f;
        [Tooltip("How much _DissolveAmount oscillates post-dissolve. Tiny value keeps the edge alive without visibly shifting.")]
        [SerializeField] private float edgeBreathRange = 0.025f;

        // Use the center from our existing Utils or define here
        private Vector2 _center = new Vector2(10.5f, -5.0f);


        private void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Clears all tiles in a specific quadrant by setting Alpha to 0.
        /// Quadrant follows the 1-4 Clockwise logic.
        /// </summary>
        public void HideQuadrant(int targetQuadrant)
        {
            // Play dissolve SFX once as the destruction begins
            if (audioSource != null && dissolveClip != null)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(dissolveClip);
            }

            // Destroy props in this quadrant immediately
            if (propsParent != null)
            {
                foreach (Transform child in propsParent)
                {
                    if (NavigationUtils.GetQuadrant(child.position) == targetQuadrant)
                        Destroy(child.gameObject);
                }
            }

            // Destroy individually tracked objects (e.g. tilemap-based buildings)
            foreach (GameObject obj in trackedObjects)
            {
                if (obj != null && NavigationUtils.GetQuadrant(GetObjectCenter(obj)) == targetQuadrant)
                    Destroy(obj);
            }

            foreach (Tilemap map in targetTilemaps)
            {
                if (map == null) continue;

                // Get the bounds of the tilemap to know what area to loop through
                BoundsInt bounds = map.cellBounds;

                foreach (Vector3Int pos in bounds.allPositionsWithin)
                {
                    if (!map.HasTile(pos)) continue;

                    // Convert cell position to World Position to check against center
                    Vector3 worldPos = map.GetCellCenterWorld(pos);

                    // Use our existing Quadrant logic
                    int tileQuad = NavigationUtils.GetQuadrant(worldPos);

                    if (tileQuad == targetQuadrant)
                    {
                        SpawnDissolve(map, pos);
                    }
                }
            }

            Debug.Log($"<color=cyan>MapDestroyer:</color> Dissolving tiles in Quadrant {targetQuadrant}.");
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------

        /// <summary>
        /// Returns the world-space center of an object.
        /// For tilemap-based buildings the transform.position is the Grid origin
        /// (often 0,0,0), so we read the Tilemap cell bounds instead.
        /// </summary>
        private static Vector3 GetObjectCenter(GameObject obj)
        {
            // Tilemap-based building: use the tile bounds center
            Tilemap tm = obj.GetComponentInChildren<Tilemap>();
            if (tm != null)
            {
                tm.CompressBounds();
                return tm.transform.TransformPoint(tm.localBounds.center);
            }

            // Sprite/mesh object: use the renderer bounds center
            Renderer rend = obj.GetComponentInChildren<Renderer>();
            if (rend != null)
                return rend.bounds.center;

            // Fallback
            return obj.transform.position;
        }

        // ---------------------------------------------------------------
        // Dissolve helpers
        // ---------------------------------------------------------------

        /// <summary>
        /// Removes a tile from the Tilemap immediately (so the player can't
        /// stand on it) and spawns a temporary "ghost" sprite that visually
        /// dissolves away using the Custom/SpriteDissolve shader.
        /// </summary>
        private void SpawnDissolve(Tilemap map, Vector3Int pos)
        {
            // Bail out if no prefab is assigned — fall back to instant hide
            if (dissolvePrefab == null)
            {
                map.SetTileFlags(pos, TileFlags.None);
                map.SetColor(pos, new Color(1, 1, 1, 0));
                return;
            }

            // Read the sprite before the tile is removed
            Sprite tileSprite = map.GetSprite(pos);
            if (tileSprite == null) return;

            Vector3 worldPos = map.GetCellCenterWorld(pos);

            // Remove the real tile so it has no collider / physics presence
            map.SetTile(pos, null);

            // Spawn the ghost and configure its SpriteRenderer
            GameObject ghost = Instantiate(dissolvePrefab, worldPos, Quaternion.identity);
            SpriteRenderer sr = ghost.GetComponent<SpriteRenderer>();

            if (sr == null)
            {
                Destroy(ghost);
                return;
            }

            sr.sprite       = tileSprite;
            sr.sortingOrder = 1; // render on top of neighbouring tiles

            StartCoroutine(RunDissolve(sr));
        }

        /// <summary>
        /// Drives the _DissolveAmount shader property from 0 → 1 over
        /// (1 / dissolveSpeed) seconds, then destroys the ghost object.
        /// </summary>
        private System.Collections.IEnumerator RunDissolve(SpriteRenderer sr)
        {
            Material mat      = sr.material; // instance copy — safe to mutate
            float    progress = 0f;

            // Multiply by dissolveStopAt so the animation always takes the same
            // wall-clock time (1/dissolveSpeed seconds) regardless of the stop value.
            float scaledSpeed = dissolveSpeed * dissolveStopAt;

            while (progress < dissolveStopAt)
            {
                progress += Time.deltaTime * scaledSpeed;
                mat.SetFloat("_DissolveAmount", Mathf.Clamp01(progress));
                yield return null;
            }

            // After dissolve settles: keep the coroutine alive with a tiny breath loop.
            // This marks the material dirty every frame so Unity re-evaluates the shader,
            // which in turn keeps the _Time-driven fire flicker running indefinitely.
            while (true)
            {
                float breath = dissolveStopAt + Mathf.Sin(Time.time * edgeBreathSpeed) * edgeBreathRange;
                mat.SetFloat("_DissolveAmount", Mathf.Clamp01(breath));
                yield return null;
            }
        }
    }
}
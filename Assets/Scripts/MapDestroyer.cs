using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheMasterPath.Utilities
{
    public class MapDestroyer : MonoBehaviour
    {

        public static MapDestroyer Instance; // Accessible from anywhere

    

        [Header("Targeting")]
        [SerializeField] private Tilemap[] targetTilemaps;

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
                        // Set the tile color to transparent (Alpha = 0)
                        // Note: TilemapRenderer 'Tile Flags' must allow color changes
                        map.SetTileFlags(pos, TileFlags.None);
                        map.SetColor(pos, new Color(1, 1, 1, 0));
                    }
                }
            }

            Debug.Log($"<color=cyan>MapDestroyer:</color> Hidden all tiles in Quadrant {targetQuadrant}");
        }
    }
}
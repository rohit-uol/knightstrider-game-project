using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheMasterPath
{
    public class Rotation : MonoBehaviour
    {
        [SerializeField] GameObject player;
        [SerializeField] Tilemap tilemap;
        [SerializeField] Tile pathTile;
        [SerializeField] Tile otherTile;
        [SerializeField] Tile highlightedTile;
        [SerializeField] Tile hiddenPathTile;

        void Start()
        {
            tilemap.CompressBounds();
            var bounds = tilemap.cellBounds;
            var block = tilemap.GetTilesBlock(bounds);
            Debug.Log(bounds);
            CopyAndRotate(bounds, block, rotateX: true, rotateY: false);
            CopyAndRotate(bounds, block, rotateX: false, rotateY: true);
            CopyAndRotate(bounds, block, rotateX: true, rotateY: true);
        }

        void CopyAndRotate(BoundsInt bounds, TileBase[] block, bool rotateX, bool rotateY)
        {
            for (int x = 0; x < bounds.size.x; x++)
            {
                for (int y = 0; y < bounds.size.y; y++)
                {
                    var oldCell = y * bounds.size.x + x;
                    var oldTile = block[oldCell];
                    var newCell = new Vector3Int(
                        x - bounds.size.x / 2,
                        y - bounds.size.y / 2 ,
                        0
                    );

                    if (rotateX)
                    {
                        newCell.x = bounds.size.x - newCell.x - 1;
                    }

                    if (rotateY)
                    {
                        newCell.y = -bounds.size.y - newCell.y - 1;
                    }

                    var tile = oldTile != pathTile ? oldTile : hiddenPathTile;
                    tilemap.SetTile(newCell, tile);
                }
            }
        }

        void Update()
        {
            var cellPosition = tilemap.WorldToCell(player.transform.position);
            var tile = tilemap.GetTile(cellPosition);
            if (tile == pathTile || tile == hiddenPathTile)
            {
                tilemap.SetTile(cellPosition, highlightedTile);
            }
        }
    }
}
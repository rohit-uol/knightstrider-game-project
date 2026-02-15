using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheMasterPath
{ 
    public class Footsteps : MonoBehaviour
    {
        [SerializeField] Movement movement;
        [SerializeField] Tilemap tilemap;
        [SerializeField] Tile tile;

        void Start()
        {
            movement.StepStarted += OnStepStarted;    
        }

        private void OnStepStarted(Vector2 stepStart, Vector2 stepEnd)
        {
            var cell = tilemap.WorldToCell(stepStart);

            tilemap.SetTile(cell, tile);

            var dir = (stepEnd - stepStart).normalized;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            tilemap.SetTransformMatrix(cell, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, angle), Vector3.one));
        }
    }
}
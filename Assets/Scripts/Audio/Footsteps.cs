using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheMasterPath
{ 
    public class Footsteps : MonoBehaviour
    {
        [SerializeField] Movement movement;
        [SerializeField] Tilemap footstepsTilemap;
        [SerializeField] Tilemap masterPathTilemap;
        [SerializeField] Tile correctTile;
        [SerializeField] Tile incorrectTile;

        void Start()
        {
            movement.StepStarted += OnStepStarted;    
        }

        private void OnStepStarted(Vector2 stepStart, Vector2 stepEnd)
        {
            var cell = footstepsTilemap.WorldToCell(stepStart);

            Tile footsteps = correctTile;
            if (masterPathTilemap.GetTile(cell) == null)
            {
                footsteps = incorrectTile;
            }
            footstepsTilemap.SetTile(cell, footsteps);

            var dir = (stepEnd - stepStart).normalized;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            footstepsTilemap.SetTransformMatrix(cell, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, angle), Vector3.one));
        }
    }
}
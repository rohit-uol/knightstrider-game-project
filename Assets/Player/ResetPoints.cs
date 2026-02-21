using UnityEngine;

namespace TheMasterPath
{
    public class ResetPoints : MonoBehaviour
    {
        [Header("Reset Reference Points")]
        [Tooltip("Assign 4 GameObjects here to act as the reset markers")]
        [SerializeField]
        private Transform[] resetTransforms = new Transform[4];

        // We store the actual positions here at start
        private Vector2[] resetPositions = new Vector2[4];

        private void Start()
        {
            // Capture the positions of the assigned GameObjects at the start of the game
            for (int i = 0; i < resetTransforms.Length; i++)
            {
                if (resetTransforms[i] != null)
                {
                    resetPositions[i] = resetTransforms[i].position;
                }
                else
                {
                    Debug.LogWarning($"ResetPoint at index {i} is missing an assigned GameObject!");
                }
            }
        }

        public Vector2 Get(Vector2 position)
        {
            var x = position.x + transform.position.x < 0f ? 0 : 1;
            var y = position.y + transform.position.y < 0f ? 0 : 1;

            var index = y * 2 + x;

            // Return the captured position from the start of the game
            return resetPositions[index];
        }
    }
}
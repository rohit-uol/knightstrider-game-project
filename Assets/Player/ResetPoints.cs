using UnityEngine;

namespace TheMasterPath
{ 
    public class ResetPoints : MonoBehaviour
    {
        [SerializeField]
        Vector2[] resetPoints = new Vector2[4];

        public Vector2 Get(Vector2 position)
        {
            var x = position.x + transform.position.x < 0f ? 0 : 1;
            var y = position.y + transform.position.y < 0f ? 0 : 1;

            var index = y * 2 + x;

            return resetPoints[index];
        }
    }
}
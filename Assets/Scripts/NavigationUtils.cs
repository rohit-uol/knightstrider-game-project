using UnityEngine;

namespace TheMasterPath.Utilities
{
    public static class NavigationUtils
    {

        private static readonly Vector2 _center = new Vector2(0f, 0.5f);

        /// <summary>
        /// Returns the quadrant (1-4) moving CLOCKWISE.
        /// 1: Top-Right, 2: Bottom-Right, 3: Bottom-Left, 4: Top-Left
        /// </summary>
        public static int GetQuadrant(Vector2 position)
        {

            Vector2 relativePos = position - _center;

            // 1. Top-Left (- , +)
            if (relativePos.x <= 0 && relativePos.y > 0) return 1;

            // 2. Top-Right (+ , +)
            if (relativePos.x > 0 && relativePos.y >= 0) return 2;

            // 3. Bottom-Right (+ , -)
            if (relativePos.x >= 0 && relativePos.y < 0) return 3;

            // 4. Bottom-Left (- , -)
            if (relativePos.x < 0 && relativePos.y <= 0) return 4;

            return 0; // Exactly at (0,0)
        }
    }
}
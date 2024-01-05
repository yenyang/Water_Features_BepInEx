namespace Water_Features.Utils
{
    using Unity.Mathematics;
    using UnityEngine;

    /// <summary>
    /// Some public static methods used for Water Tool.
    /// </summary>
    public static class WaterSourceUtils
    {
        /// <summary>
        /// Gets a radius for area for interacting with the water source.
        /// </summary>
        /// <param name="position">The transform position for the water source.</param>
        /// <param name="radius">The radius of the water source.</param>
        /// <param name="mapExtents">A float representing the maximum extents of the map.</param>
        /// <returns>A radius for displaying the water source.</returns>
        public static float GetRadius(float3 position, float radius, float mapExtents)
        {
            float minRadius = 25f;
            float maxRadius = 150f;
            if (Mathf.Max(Mathf.Abs(position.x), Mathf.Abs(position.z)) > mapExtents + 100f)
            {
                maxRadius = Mathf.Max(Mathf.Abs(position.x), Mathf.Abs(position.z)) - mapExtents + 100f;
            }

            return Mathf.Clamp(radius, minRadius, maxRadius);
        }

        /// <summary>
        /// Checks whether the cursor is hovering over a water source to interact with it. 
        /// </summary>
        /// <param name="cursorPosition">From the raycast.</param>
        /// <param name="waterSourcePosition">The transform position of the water source.</param>
        /// <param name="radius">Radius of the water source.</param>
        /// <param name="mapExtents">A float representing the maximum extents of the map.</param>
        /// <returns>True if the cursor is over the interactiable area of the water source. False if not.</returns>
        public static bool CheckForHoveringOverWaterSource(float3 cursorPosition, float3 waterSourcePosition, float radius, float mapExtents)
        {
            radius = GetRadius(waterSourcePosition, radius, mapExtents);
            if (Unity.Mathematics.math.distance(cursorPosition, waterSourcePosition) < radius)
            {
                return true;
            }

            return false;
        }
    }
}

using facingfate;
using System.Collections.Generic;
using UnityEngine;


namespace facingfate
{
    /// <summary>
    /// Runtime debug visualization for targeting effects during gameplay.
    /// Uses Debug.DrawLine for frame-based visualization that works during play mode.
    /// </summary>
    public static class DebugVisualization
    {
        private static bool isEnabled = true;

        /// <summary>
        /// Enable or disable debug visualization globally.
        /// </summary>
        public static void SetEnabled(bool enabled)
        {
            isEnabled = enabled;
        }

        /// <summary>
        /// Visualizes a targeting sphere.
        /// </summary>
        public static void DrawSphere(Vector3 center, float radius, Color color, float duration = 0f)
        {
            if (!isEnabled) return;
            DrawCircle(center, Vector3.up, radius, color, duration);
            DrawCircle(center, Vector3.right, radius, color, duration);
            DrawCircle(center, Vector3.forward, radius, color, duration);
        }

        /// <summary>
        /// Visualizes a targeting cone.
        /// </summary>
        public static void DrawCone(Vector3 apex, Vector3 direction, float range, float coneAngle, Color color, float duration = 0f)
        {
            if (!isEnabled) return;

            direction = direction.normalized;
            float halfAngle = coneAngle * 0.5f;

            // Draw cone end circle
            Vector3 coneEnd = apex + direction * range;
            float coneRadiusAtEnd = range * Mathf.Tan(halfAngle * Mathf.Deg2Rad);

            Vector3 perpendicular1 = Vector3.Cross(direction, Vector3.up);
            if (perpendicular1.magnitude < 0.1f)
                perpendicular1 = Vector3.Cross(direction, Vector3.right);
            perpendicular1 = perpendicular1.normalized;
            Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular1).normalized;

            DrawCircle(coneEnd, perpendicular1, perpendicular2, coneRadiusAtEnd, color, duration);

            // Draw lines from apex to cone edge
            Vector3 edgePoint1 = coneEnd + perpendicular1 * coneRadiusAtEnd;
            Vector3 edgePoint2 = coneEnd - perpendicular1 * coneRadiusAtEnd;
            Vector3 edgePoint3 = coneEnd + perpendicular2 * coneRadiusAtEnd;
            Vector3 edgePoint4 = coneEnd - perpendicular2 * coneRadiusAtEnd;

            Debug.DrawLine(apex, edgePoint1, color, duration);
            Debug.DrawLine(apex, edgePoint2, color, duration);
            Debug.DrawLine(apex, edgePoint3, color, duration);
            Debug.DrawLine(apex, edgePoint4, color, duration);
        }

        /// <summary>
        /// Visualizes a line area (capsule).
        /// </summary>
        public static void DrawCapsule(Vector3 start, Vector3 end, float radius, Color color, float duration = 0f)
        {
            if (!isEnabled) return;

            // Draw main line
            Debug.DrawLine(start, end, color, duration);

            // Draw circles at start and end
            Vector3 direction = (end - start).normalized;
            Vector3 perpendicular1 = Vector3.Cross(direction, Vector3.up);
            if (perpendicular1.magnitude < 0.1f)
                perpendicular1 = Vector3.Cross(direction, Vector3.right);
            perpendicular1 = perpendicular1.normalized;
            Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular1).normalized;

            DrawCircle(start, perpendicular1, perpendicular2, radius, color, duration);
            DrawCircle(end, perpendicular1, perpendicular2, radius, color, duration);
        }

        /// <summary>
        /// Visualizes targeted entity positions as markers.
        /// </summary>
        public static void DrawEntityMarkers(List<Vector3> positions, Color color, float markerSize = 0.3f, float duration = 0f)
        {
            if (!isEnabled || positions == null) return;

            foreach (var pos in positions)
            {
                DrawCross(pos, markerSize, color, duration);
            }
        }

        /// <summary>
        /// Visualizes the complete targeting mode data.
        /// </summary>
        public static void DrawTargetingData(TargetingModeData data, CardTargetingMode mode, CardData cardData, Color effectColor, float duration = 0f)
        {
            if (!isEnabled || data == null || cardData == null) return;

            Vector3 castOrigin = data.castingPosition;

            switch (mode)
            {
                case CardTargetingMode.Radius:
                    // Radius is centered on the aimed position, not the caster
                    if (data.targetedPositions.Count > 0)
                    {
                        Vector3 radiusCenter = data.targetedPositions[0];
                        DrawSphere(radiusCenter, cardData.Radius, effectColor, duration);
                        DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    }
                    break;

                case CardTargetingMode.Ring:
                    // Ring is centered on the aimed position, not the caster
                    if (data.targetedPositions.Count > 0)
                    {
                        Vector3 ringCenter = data.targetedPositions[0];
                        DrawSphere(ringCenter, cardData.Radius, effectColor, duration);
                        DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    }
                    break;

                case CardTargetingMode.Cone:
                    if (data.targetedPositions.Count > 0)
                    {
                        Vector3 direction = (data.targetedPositions[0] - castOrigin).normalized;
                        DrawCone(castOrigin, direction, cardData.Range, cardData.Area, effectColor, duration);
                        DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    }
                    break;

                case CardTargetingMode.LineSelf:
                    // Draw line from caster's position along the targeting direction
                    if (data.targetedPositions.Count > 0)
                    {
                        Vector3 direction = (data.targetedPositions[0] - castOrigin).normalized;
                        DrawCapsule(castOrigin, castOrigin + direction * cardData.Range, cardData.Area, effectColor, duration);
                    }
                    else
                    {
                        Vector3 direction = Vector3.forward;
                        DrawCapsule(castOrigin, castOrigin + direction * cardData.Range, cardData.Area, effectColor, duration);
                    }
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    break;

                case CardTargetingMode.LineFree:
                    // Draw line between selected target positions
                    if (data.targetedPositions.Count > 1)
                    {
                        DrawCapsule(data.targetedPositions[0], data.targetedPositions[data.targetedPositions.Count - 1], cardData.Area, effectColor, duration);
                    }
                    else if (data.targetedPositions.Count == 1)
                    {
                        // Fallback: if only one position, draw a small indicator
                        DrawSphere(data.targetedPositions[0], cardData.Area * 0.5f, effectColor, duration);
                    }
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    break;

                case CardTargetingMode.Single:
                    DrawSphere(castOrigin, 0.3f, effectColor, duration);
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    break;

                case CardTargetingMode.Select:
                    DrawSphere(castOrigin, cardData.Radius, effectColor, duration);
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    break;

                case CardTargetingMode.All:
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    break;
            }
        }

        /// <summary>
        /// Helper to draw a circle in 3D space using Debug.DrawLine.
        /// </summary>
        private static void DrawCircle(Vector3 center, Vector3 axis, float radius, Color color, float duration = 0f, int segments = 20)
        {
            Vector3 perpendicular1 = Vector3.Cross(axis, Vector3.up);
            if (perpendicular1.magnitude < 0.1f)
                perpendicular1 = Vector3.Cross(axis, Vector3.right);
            perpendicular1 = perpendicular1.normalized;

            DrawCircle(center, perpendicular1, Vector3.Cross(axis, perpendicular1).normalized, radius, color, duration, segments);
        }

        /// <summary>
        /// Helper to draw a circle defined by two perpendicular axes.
        /// </summary>
        private static void DrawCircle(Vector3 center, Vector3 axisX, Vector3 axisY, float radius, Color color, float duration = 0f, int segments = 20)
        {
            Vector3 lastPoint = center + axisX * radius;
            for (int i = 1; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 newPoint = center + (Mathf.Cos(angle) * axisX + Mathf.Sin(angle) * axisY) * radius;
                Debug.DrawLine(lastPoint, newPoint, color, duration);
                lastPoint = newPoint;
            }
        }

        /// <summary>
        /// Helper to draw a cross/plus marker at a position.
        /// </summary>
        private static void DrawCross(Vector3 position, float size, Color color, float duration = 0f)
        {
            Debug.DrawLine(position - Vector3.right * size, position + Vector3.right * size, color, duration);
            Debug.DrawLine(position - Vector3.up * size, position + Vector3.up * size, color, duration);
            Debug.DrawLine(position - Vector3.forward * size, position + Vector3.forward * size, color, duration);
        }
    }
}
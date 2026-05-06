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
        /// Visualizes a targeting cone as a flat pie-slice in the XZ plane.
        /// </summary>
        public static void DrawCone(Vector3 origin, Vector3 target, float range, float coneAngle, Color color, float duration = 0f)
        {
            if (!isEnabled) return;

            // Flatten to XZ plane
            target = new Vector3(target.x, 0f, target.z).normalized;
            if (target == Vector3.zero) target = Vector3.forward;

            // Two edge rays
            Vector3 leftEdge = Quaternion.AngleAxis(-coneAngle, Vector3.up) * target;
            Vector3 rightEdge = Quaternion.AngleAxis(coneAngle, Vector3.up) * target;
            Vector3 leftTip = origin + leftEdge * range;
            Vector3 rightTip = origin + rightEdge * range;

            Debug.DrawLine(origin, leftTip, color, duration);
            Debug.DrawLine(origin, rightTip, color, duration);

            // Arc at range
            int arcSegments = Mathf.Clamp(Mathf.RoundToInt(coneAngle * 2f / 5f), 6, 36);
            Vector3 prev = leftTip;
            for (int i = 1; i <= arcSegments; i++)
            {
                float t = (float)i / arcSegments;
                float angle = Mathf.Lerp(-coneAngle, coneAngle, t);
                Vector3 dir = Quaternion.AngleAxis(angle, Vector3.up) * target;
                Vector3 next = origin + dir * range;
                Debug.DrawLine(prev, next, color, duration);
                prev = next;
            }
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
            Vector3 aimOrigin = data.aimPosition;

            switch (mode)
            {
                case CardTargetingMode.Sphere:

                    DrawSphere(aimOrigin, cardData.Radius, effectColor, duration);
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);

                    break;

                case CardTargetingMode.Ring:

                    DrawRing(aimOrigin, cardData.Radius, cardData.Radius + cardData.Area, effectColor, duration);
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    DrawRingEntityLines(aimOrigin, cardData.Radius, cardData.Radius + cardData.Area, duration);

                    break;

                case CardTargetingMode.RingSelf:
                    // Ring is centered on the caster's position (aimOrigin == castOrigin for self)
                    DrawRing(aimOrigin, cardData.Radius, cardData.Radius + cardData.Area, effectColor, duration);
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    DrawRingEntityLines(aimOrigin, cardData.Radius, cardData.Radius + cardData.Area, duration);
                    break;

                case CardTargetingMode.Cone:
                    {
                        Vector3 coneDirection = (aimOrigin - castOrigin).normalized;
                        if (coneDirection == Vector3.zero) coneDirection = Vector3.forward;
                        DrawCone(castOrigin, coneDirection, cardData.Range, 45, effectColor, duration);
                        DrawConeRays(castOrigin, coneDirection, cardData.Range, 45, duration);
                        DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    }
                    break;

                case CardTargetingMode.LineSelf:
                    // Draw line from caster toward the aimed position
                    {
                        Vector3 lineDirection = (aimOrigin - castOrigin).normalized;
                        if (lineDirection == Vector3.zero) lineDirection = Vector3.forward;
                        DrawCapsule(castOrigin, castOrigin + lineDirection * cardData.Range, cardData.Area, effectColor, duration);
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
                    DrawSphere(aimOrigin, 0.3f, effectColor, duration);
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    break;

                case CardTargetingMode.Select:
                    DrawSphere(aimOrigin, cardData.Radius, effectColor, duration);
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    break;

                case CardTargetingMode.SelectionUnique:
                    DrawSphere(aimOrigin, cardData.Radius, effectColor, duration);
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    break;

                case CardTargetingMode.All:
                    DrawEntityMarkers(data.targetedPositions, Color.green, 0.2f, duration);
                    break;
            }
        }

        /// <summary>
        /// Casts a flat fan of rays in the XZ plane matching the cone detection logic.
        /// Green up to a hit, faint white for the remainder. Ray count scales with range.
        /// </summary>
        public static void DrawConeRays(Vector3 origin, Vector3 direction, float range, float coneAngle, float duration = 0f)
        {
            if (!isEnabled) return;

            direction = new Vector3(direction.x, 0f, direction.z).normalized;
            if (direction == Vector3.zero) direction = Vector3.forward;

            int rayCount = Mathf.Clamp(Mathf.RoundToInt(range * 4f), 8, 64);

            for (int i = 0; i <= rayCount; i++)
            {
                float t = (float)i / rayCount;
                float angle = Mathf.Lerp(-coneAngle, coneAngle, t);
                Vector3 rayDir = Quaternion.AngleAxis(angle, Vector3.up) * direction;
                DrawRay(origin, rayDir, range, duration);
            }
        }

        private static void DrawRay(Vector3 origin, Vector3 direction, float range, float duration)
        {
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit, range))
            {
                Debug.DrawLine(origin, hit.point, Color.green, duration);
                Debug.DrawLine(hit.point, origin + direction * range, new Color(1f, 1f, 1f, 0.2f), duration);
            }
            else
            {
                Debug.DrawLine(origin, origin + direction * range, new Color(1f, 1f, 1f, 0.2f), duration);
            }
        }

        /// <summary>
        /// Draws a line from the ring center to each entity position.
        /// Green if the entity is within the ring area, red otherwise.
        /// </summary>
        public static void DrawRingEntityLines(Vector3 center, float innerRadius, float outerRadius, float duration = 0f)
        {
            if (!isEnabled) return;

            var allEntities = TargetingUtility.AllEntitiesCache();

            foreach (var entity in allEntities)
            {
                if (entity == null) continue;
                Vector3 pos = entity.transform.position;
                float dist = Vector3.Distance(center, pos);
                bool inRing = dist >= innerRadius && dist <= outerRadius;
                Color lineColor = inRing ? Color.green : Color.red;
                Debug.DrawLine(center, pos, lineColor, duration);
            }
        }

        /// <summary>
        /// Visualizes a ring (annulus) with inner and outer radii.
        /// </summary>
        public static void DrawRing(Vector3 center, float innerRadius, float outerRadius, Color color, float duration = 0f)
        {
            if (!isEnabled) return;
            DrawCircle(center, Vector3.up, innerRadius, color, duration);
            DrawCircle(center, Vector3.up, outerRadius, color, duration);

            // Draw radial connector lines to make the ring shape clear
            int connectors = 8;
            for (int i = 0; i < connectors; i++)
            {
                float angle = (i / (float)connectors) * Mathf.PI * 2f;
                Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                Debug.DrawLine(center + dir * innerRadius, center + dir * outerRadius, color, duration);
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
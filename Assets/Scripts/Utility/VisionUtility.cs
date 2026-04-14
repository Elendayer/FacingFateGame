using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class VisionUtility
{
    private const float VisionRayHeight = 0.5f;
    private const float VisionCheckDistance = 100f;
    private const int VisionLayerMask = ~0; // Check all layers by default

    /// <summary>
    /// Returns all tiles from tilesToCheck that have line-of-sight from origin
    /// using physics raycasts and navmesh sampling to determine visibility.
    /// </summary>
    public static List<Vector3> GetVisibleTiles(
        Vector3 originOffset,
        List<Vector3> tilesToCheckOffset
    )
    {
        List<Vector3> result = new();

        foreach (var targetOffset in tilesToCheckOffset)
        {
            if (HasLineOfSight(originOffset, targetOffset))
                result.Add(targetOffset);
        }

        return result;
    }

    /// <summary>
    /// Performs line-of-sight check using physics raycasts and navmesh queries.
    /// Returns true if there's a clear line of sight between origin and target.
    /// </summary>
    private static bool HasLineOfSight(Vector3 origin, Vector3 target)
    {
        if (origin == target)
            return true;

        // Sample both positions on the navmesh to verify they're in walkable areas
        NavMeshHit originHit, targetHit;
        if (!NavMesh.SamplePosition(origin, out originHit, 5f, NavMesh.AllAreas))
            return false;
        if (!NavMesh.SamplePosition(target, out targetHit, 5f, NavMesh.AllAreas))
            return false;

        Vector3 originRayStart = originHit.position + Vector3.up * VisionRayHeight;
        Vector3 targetRayEnd = targetHit.position + Vector3.up * VisionRayHeight;

        // Cast ray from origin to target
        RaycastHit hit;
        Vector3 direction = (targetRayEnd - originRayStart).normalized;
        float distance = Vector3.Distance(originRayStart, targetRayEnd);

        if (Physics.Raycast(originRayStart, direction, out hit, distance, VisionLayerMask))
        {
            // Ray was blocked by something
            return false;
        }

        // Verify both positions are on the same navmesh
        NavMeshPath path = new NavMeshPath();
        if (!NavMesh.CalculatePath(originHit.position, targetHit.position, NavMesh.AllAreas, path))
            return false;

        return path.status == NavMeshPathStatus.PathComplete;
    }
}

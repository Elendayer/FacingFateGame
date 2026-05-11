using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using facingfate;
using UnityEngine.AI;


public static class TargetingUtility
{
    // Set to false in Release builds to eliminate debug log overhead
#if UNITY_EDITOR
    private const bool ENABLE_TARGETING_LOGGING = false;
#else
    private const bool ENABLE_TARGETING_LOGGING = false;
#endif

    #region Entity Validation
    public static List<EntityScript> GetValidTargets(CardData card, EntityScript overrideOwner = null)
    {
        var candidates = AllEntitiesCache();

        if (card == null || (card.Owner == null && overrideOwner == null) || candidates == null) return new List<EntityScript>();

        var owner = overrideOwner ?? card.Owner;
        var results = new List<EntityScript>(candidates.Count);
        for (int i = 0; i < candidates.Count; i++)
        {
            var target = candidates[i];
            if (target != null && IsTargetValid(card, target, owner))
            {
                results.Add(target);
            }
        }
        return results;
    }
    public static bool IsTargetValid(CardData cardData, EntityScript target, EntityScript overrideOwner = null)
    {
        if (target == null) return false;
        if (target.enabled == false) return false;

        EntityScript owner = overrideOwner ?? cardData.Owner;
        if (owner == null) return false;

        var targeting = cardData.targetingData;
        var targetAff = target.entityAffiliation;
        var ownerAff = owner.entityAffiliation;

        if (target.HasReference(GameplayRef.untargetableByAll).found) return false;

        bool baseValid = targeting.CardTargetAffiliation switch
        {
            CardTargetAffiliation.Ally => targetAff == ownerAff,
            CardTargetAffiliation.Enemy => targetAff != ownerAff,
            CardTargetAffiliation.Self => target == owner,
            CardTargetAffiliation.All => true,
            CardTargetAffiliation.None => false,
            _ => false
        };

        if (!baseValid) return false;

        bool isEnemyTarget = targetAff != ownerAff && targetAff != EntityAffiliation.Neutral;
        bool isAllyTarget = targetAff == ownerAff && targetAff != EntityAffiliation.Neutral;

        if (isEnemyTarget && target.HasReference(GameplayRef.untargetableByEnemies).found) return false;

        if (isAllyTarget && target.HasReference(GameplayRef.untargetableByAllies).found) return false;

        return true;
    }

    /// <summary>
    /// Validates an entity as a valid drag/click target during card targeting.
    /// Includes all base validity checks plus additional requirements: active state, range, and health.
    /// </summary>
    public static bool IsDraggableTargetValid(CardData cardData, EntityScript target, EntityScript overrideOwner = null)
    {
        // First check base targeting validity
        if (!IsTargetValid(cardData, target, overrideOwner))
            return false;

        // Check if entity is alive/active
        if (target.gameObject == null || !target.gameObject.activeInHierarchy)
            return false;

        // Check if entity is within card range
        EntityScript owner = overrideOwner ?? cardData.Owner;
        if (owner != null && cardData != null)
        {
            float distanceToEntity = Vector3.Distance(owner.transform.position, target.transform.position);
            if (distanceToEntity > cardData.Range)
                return false;
        }

        // Check if entity has valid stats (not already dead)
        if (target.entityStats != null && target.entityStats.CurrentHealth <= 0)
            return false;

        return true;
    }

    public static bool isEnemyOf(EntityScript a, EntityScript b)
    {
        if (a == null || b == null) return false;
        if (a.entityAffiliation == EntityAffiliation.Neutral || b.entityAffiliation == EntityAffiliation.Neutral) return false;
        return a.entityAffiliation != b.entityAffiliation;
    }
    #endregion

    #region Physics-Based Targeting
    /// <summary>
    /// Gets entities within a physics sphere at the given world position.
    /// </summary>
    public static List<EntityScript> GetEntitiesInPhysicsSphere(Vector3 worldPos, float radius, CardData cardData = null)
    {
        Collider[] colliders = Physics.OverlapSphere(worldPos, radius);
        List<EntityScript> results = new();

        foreach (var collider in colliders)
        {
            var entity = collider.GetComponent<EntityScript>();
            if (entity != null)
            {
                if (cardData != null && !IsTargetValid(cardData, entity))
                    continue;
                results.Add(entity);
            }
        }
        return results;
    }

    public static List<EntityScript> GetEntitiesInPhysicsRing(Vector3 worldPos, float innerRadius, float outerRadius, CardData cardData = null)
    {
        Collider[] colliders = Physics.OverlapSphere(worldPos, outerRadius);

        List<EntityScript> results = new();

        foreach (var collider in colliders)
        {
            var entity = collider.GetComponent<EntityScript>();
            if (entity == null) continue;

            float dist = Vector3.Distance(worldPos, entity.transform.position);

#if UNITY_EDITOR
            if (ENABLE_TARGETING_LOGGING) Debug.Log($"Entity {entity.name} at distance {dist:F2} from center. InnerRadius: {innerRadius}, OuterRadius: {outerRadius}");
#endif

            // Include targets within the ring (between inner and outer radius)
            if (dist < innerRadius || dist > outerRadius)
                continue;


            if (cardData != null && !IsTargetValid(cardData, entity))
                continue;

#if UNITY_EDITOR
            if (ENABLE_TARGETING_LOGGING) Debug.Log("Valid target in ring!");
#endif

            results.Add(entity);
        }

#if UNITY_EDITOR
        if (ENABLE_TARGETING_LOGGING) Debug.Log(results.Count);
#endif
        return results;
    }

    /// <summary>
    /// Gets entities within a physics cone by casting a flat fan of rays in the XZ plane.
    /// Ray count scales with range to keep consistent angular density.
    /// </summary>
    public static List<EntityScript> GetEntitiesInPhysicsCone(Vector3 origin, Vector3 direction, float range, float coneAngle, CardData cardData = null)
    {
        // Flatten to XZ plane — targets sit at ground level (y = 0)
        direction = new Vector3(direction.x, 0f, direction.z).normalized;
        if (direction == Vector3.zero) direction = Vector3.forward;

        // Scale ray count with range: ~4 rays per unit, clamped 8–64
        int rayCount = Mathf.Clamp(Mathf.RoundToInt(range * 4f), 8, 64);

        HashSet<EntityScript> hitSet = new();
        int layerMask = ~0;

        for (int i = 0; i <= rayCount; i++)
        {
            float t = (float)i / rayCount;
            float angle = Mathf.Lerp(-coneAngle, coneAngle, t);
            Vector3 rayDir = Quaternion.AngleAxis(angle, Vector3.up) * direction;

            var hits = Physics.RaycastAll(origin, rayDir, range, layerMask);
            foreach (var hit in hits)
            {
                var entity = hit.collider.GetComponentInParent<EntityScript>();
                if (entity == null || !entity.enabled) continue;
                if (cardData != null && !IsTargetValid(cardData, entity)) continue;
                hitSet.Add(entity);
            }
        }

        return new List<EntityScript>(hitSet);
    }

    /// <summary>
    /// Checks line of sight using physics raycasts instead of tile-based checks.
    /// </summary>
    public static bool HasPhysicsLineOfSight(Vector3 from, Vector3 to)
    {
        Vector3 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);

        // Small raycast from start position
        if (Physics.Raycast(from, direction, distance))
        {
            // Check if we hit the target or if it's beyond an obstacle
            RaycastHit hit;
            if (Physics.Raycast(from, direction, out hit, distance))
            {
                // If we hit something, check if it's close to the target
                if (Vector3.Distance(hit.point, to) < 0.5f)
                    return true;
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Gets entities in a line from caster using physics raycasts.
    /// </summary>
    public static List<EntityScript> GetEntitiesInPhysicsLine(Vector3 origin, Vector3 direction, float range, float lineWidth, CardData cardData = null)
    {
        direction = direction.normalized;

        List<EntityScript> results = new();

        Collider[] colliders = Physics.OverlapCapsule(origin, origin + direction * range, lineWidth);

        foreach (var collider in colliders)
        {
            var entity = collider.GetComponent<EntityScript>();
            if (entity != null)
            {
                if (cardData != null && !IsTargetValid(cardData, entity))
                    continue;
                results.Add(entity);
            }
        }

        return results;
    }
    #endregion

    #region Entity
    public static List<EntityScript> AllEntitiesCache()
    {
        List<EntityScript> allEntities = UnityEngine.Object.FindObjectsByType<EntityScript>(0).ToList();

        return allEntities;
    }

    public static Vector3 GetHoveredPosition(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.TryGetComponent<DraggableTarget>(out var dt) &&
                dt.draggableTargetType == DraggableTargetType.CombatTile)
            {
                return hit.point;
            }
        }
        return Vector3.zero;
    }

    public static Vector3 GetHoveredNavMesh(Ray ray)
    {
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(hit.point, out navHit, 1.0f, NavMesh.AllAreas))
            {
                return navHit.position;
            }
        }
        return Vector3.zero;
    }

    public static TargetingModeData GetAffected(CardScript card, Vector3 aimWorldPos, EntityScript owner, bool usesVision, List<Vector3> selectedPositions = null, bool isVetted = false)
    {
        List<EntityScript> entities = new();

        TargetingModeData targetingModeData = new TargetingModeData();

        CardData cardData = card.cardData;
        Vector3 castWorldPos = owner.transform.position;

        // Store caster position for visualization (will be updated for specific modes below)
        targetingModeData.castingPosition = castWorldPos;
        targetingModeData.aimPosition = aimWorldPos;

        switch (cardData.targetingData.cardTargetingMode)
        {
            case CardTargetingMode.Select:
                {
                    if (selectedPositions != null)
                    {
                        foreach (var pos in selectedPositions)
                        {
                            entities.AddRange(GetEntitiesInPhysicsSphere(pos, 0.1f, cardData));
                        }
                    }
                }
                break;

            case CardTargetingMode.SelectionUnique:
                {
                    if (selectedPositions != null)
                    {
                        HashSet<EntityScript> uniqueEntities = new();
                        foreach (var pos in selectedPositions)
                        {
                            var entitiesAtPos = GetEntitiesInPhysicsSphere(pos, 0.1f, cardData);
                            foreach (var entity in entitiesAtPos)
                            {
                                uniqueEntities.Add(entity);
                            }
                        }
                        entities = uniqueEntities.ToList();
                    }
                }
                break;
            case CardTargetingMode.LineFree:
                {
                    if (selectedPositions != null && selectedPositions.Count > 1)
                    {
                        Vector3 direction = (selectedPositions[selectedPositions.Count - 1] - selectedPositions[0]).normalized;
                        entities = GetEntitiesInPhysicsLine(selectedPositions[0], direction, cardData.Range, cardData.Area, cardData);
                    }
                }
                break;
            case CardTargetingMode.Cone:
                {
                    Vector3 direction = (aimWorldPos - castWorldPos).normalized;
                    entities = GetEntitiesInPhysicsCone(castWorldPos, direction, cardData.Range, 45, cardData);
                }
                break;
            case CardTargetingMode.LineSelf:
                {
                    Vector3 direction = (aimWorldPos - castWorldPos).normalized;
                    entities = GetEntitiesInPhysicsLine(castWorldPos, direction, cardData.Range, cardData.Area, cardData);
                }
                break;
            case CardTargetingMode.Ring:
                {
                    entities = GetEntitiesInPhysicsRing(aimWorldPos, cardData.Radius, cardData.Radius + cardData.Area, cardData);
                }
                break;
            case CardTargetingMode.RingSelf:
                {
                    // Ring centered at the caster's position, not the aim position
                    entities = GetEntitiesInPhysicsRing(castWorldPos, cardData.Radius, cardData.Radius + cardData.Area, cardData);
                }
                break;
            case CardTargetingMode.Sphere:
                {
                    entities = GetEntitiesInPhysicsSphere(aimWorldPos, cardData.Radius, cardData);
                }
                break;
            case CardTargetingMode.Single:
                {
                    entities = GetEntitiesInPhysicsSphere(aimWorldPos, 0.1f, cardData);
                }
                break;
        }

        if (usesVision && entities.Count > 0)
        {
            entities = entities.FindAll(e => HasPhysicsLineOfSight(aimWorldPos, e.transform.position));
        }

        if (isVetted)
        {
            targetingModeData.targetedEntities = entities.FindAll(e => IsTargetValid(cardData, e));
        }
        else
        {
            targetingModeData.targetedEntities = entities;
        }

        // For Ground-type cards, use the activation positions; for Entity-type, use targeted entity positions
        if (cardData.targetingData.CardTargetType == CardTargetType.Ground)
        {
            // Use selectedPositions for multi-select modes, otherwise use aimWorldPos
            if (selectedPositions != null && selectedPositions.Count > 0)
            {
                targetingModeData.targetedPositions = new List<Vector3>(selectedPositions);
            }
            else
            {
                targetingModeData.targetedPositions = new List<Vector3> { aimWorldPos };
            }
        }
        else
        {
            // Entity-type cards: use positions of targeted entities
            targetingModeData.targetedPositions = targetingModeData.targetedEntities.Select(e => e.transform.position).ToList();
        }

        // Visualize the targeting effect
        //DebugVisualization.DrawTargetingData(targetingModeData, cardData.targetingData.cardTargetingMode, cardData, Color.cyan);

        return targetingModeData;
    }

    /// <summary>
    /// Applies vision filtering to the targeting data, removing entities that are not visible.
    /// </summary>
    public static void ApplyVisionFilterToTargeting(TargetingModeData targetingData, CardData cardData)
    {
        if (targetingData?.targetedEntities == null || cardData == null)
            return;

        if (!cardData.targetingData.TargetingUsesVision)
            return;

        var casterPos = targetingData.castingPosition;
        targetingData.targetedEntities = targetingData.targetedEntities
            .FindAll(e => HasPhysicsLineOfSight(casterPos, e.transform.position))
            .ToList();
    }

    /// <summary>
    /// Validates if a targeting position is reachable with the given movement budget.
    /// </summary>
    public static bool ValidateReachability(TargetingModeData targetingData, Vector3 currentPosition, float movementBudget, EntityStats entityStats)
    {
        if (targetingData == null)
            return false;

        var pathData = MovementUtility.FindPath(currentPosition, targetingData.castingPosition, entityStats: entityStats);

        if (pathData == null || pathData.PathCost > movementBudget)
        {
            targetingData.IsReachable = false;
            return false;
        }

        targetingData.IsReachable = true;
        return true;
    }

    /// <summary>
    /// Validates and filters targeting data, applying vision filtering and reachability checks.
    /// </summary>
    public static void ValidateTargetingData(TargetingModeData targetingData, CardData cardData, Vector3 currentPosition, float movementBudget, EntityStats entityStats)
    {
        if (targetingData == null)
            return;

        // Apply vision filtering if the card uses vision
        ApplyVisionFilterToTargeting(targetingData, cardData);

        // Check reachability
        ValidateReachability(targetingData, currentPosition, movementBudget, entityStats);
    }

    /// <summary>
    /// Finds a position on the navmesh that is within card range of a target entity.
    /// For entities not on navmesh (off-turn entities), finds the closest walkable position within range.
    /// Returns null if no valid position within range can be found.
    /// </summary>
    public static Vector3? FindRangeAwarePositionForTarget(Vector3 targetPos, Vector3 ownerPos, float cardRange)
    {
        float currentDist = Vector3.Distance(ownerPos, targetPos);

#if UNITY_EDITOR
        if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] FindRangeAwarePositionForTarget called - OwnerPos: {ownerPos}, TargetPos: {targetPos}, CardRange: {cardRange}, CurrentDist: {currentDist:F2}");
#endif

        // Already in range, return the target position
        if (currentDist <= cardRange)
        {
#if UNITY_EDITOR
            if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] Target already in range! Current distance {currentDist:F2} <= card range {cardRange}");
#endif
            return targetPos;
        }

        // Out of range - need to find a position on navmesh within range
        // Strategy: Use a reference point exactly at cardRange distance from target, positioned on the line between owner and target
        // Then search around this reference point and along the approach line
        int positionsTestedCount = 0;
        const float TOLERANCE = 0.1f; // Allow slight tolerance for NavMesh snapping and floating point precision
        const float SEARCH_STEP = 0.25f; // Distance between search points

        Vector3 directionToTarget = (targetPos - ownerPos).normalized;

        // Calculate reference point: exactly cardRange away from target, on the line toward owner
        Vector3 referencePoint = targetPos - (directionToTarget * cardRange);

        // [FindPathIntoRange] Debug: Reference point
        //Debug.Log($"[FindPathIntoRange] Using reference point at: {referencePoint}, (cardRange {cardRange}m from target along owner direction)");

        // First, try the reference point itself
        if (NavMesh.SamplePosition(referencePoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            float distToTarget = Vector3.Distance(hit.position, targetPos);
            positionsTestedCount++;

            //Debug.Log($"[FindPathIntoRange] ✓ Valid navmesh position found at iteration {positionsTestedCount}: {hit.position}, dist to target: {distToTarget:F2}");

            if (distToTarget <= cardRange + TOLERANCE)
            {
                //Debug.Log($"[FindPathIntoRange] ✓✓ ACCEPTED - Reference point is within tolerance (dist: {distToTarget:F2} <= range+tolerance: {cardRange + TOLERANCE:F2})");
                return hit.position;
            }
        }

        // Search around the reference point in expanding circles and along approach line
        // Search inward toward the target (to closer positions from reference point)
        for (float offsetDist = SEARCH_STEP; offsetDist <= cardRange; offsetDist += SEARCH_STEP)
        {
            // Try along the approach line (between reference point and target, moving toward target)
            Vector3 candidate = referencePoint + (directionToTarget * offsetDist);
            positionsTestedCount++;

            if (NavMesh.SamplePosition(candidate, out hit, 2f, NavMesh.AllAreas))
            {
                float distToTarget = Vector3.Distance(hit.position, targetPos);

#if UNITY_EDITOR
                if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] ✓ Valid navmesh position found at iteration {positionsTestedCount}: {hit.position}, dist to target: {distToTarget:F2}");
#endif

                if (distToTarget <= cardRange + TOLERANCE)
                {
#if UNITY_EDITOR
                    if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] ✓✓ ACCEPTED - Position is within tolerance (dist: {distToTarget:F2} <= range+tolerance: {cardRange + TOLERANCE:F2})");
#endif
                    return hit.position;
                }
            }

            // Try in a circle around this position along the approach
            for (float angle = 0; angle < 360; angle += 45)
            {
                float rad = angle * Mathf.Deg2Rad;
                candidate = referencePoint + (directionToTarget * offsetDist) + (new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * offsetDist);
                positionsTestedCount++;

                if (NavMesh.SamplePosition(candidate, out hit, 2f, NavMesh.AllAreas))
                {
                    float distToTarget = Vector3.Distance(hit.position, targetPos);

#if UNITY_EDITOR
                    if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] ✓ Valid navmesh position found at iteration {positionsTestedCount}: {hit.position}, dist to target: {distToTarget:F2}");
#endif

                    if (distToTarget <= cardRange + TOLERANCE)
                    {
#if UNITY_EDITOR
                        if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] ✓✓ ACCEPTED - Position is within tolerance (dist: {distToTarget:F2} <= range+tolerance: {cardRange + TOLERANCE:F2})");
#endif
                        return hit.position;
                    }
                }
            }
        }

#if UNITY_EDITOR
        if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] ✗ FAILED - Tested {positionsTestedCount} positions but none were valid navmesh positions within {cardRange}m (+ {TOLERANCE}m tolerance) of target");
#endif
        return null;
    }

    /// <summary>
    /// Finds a valid navmesh position within range and calculates the path to reach it.
    /// Handles off-navmesh entities by finding the nearest walkable position within range.
    /// Returns the path data and the position to cast from, or null if unreachable.
    /// </summary>
    public static (NavMeshPathData pathData, Vector3 castPosition)? FindPathIntoRange(
        Vector3 fromPosition,
        Vector3 targetPosition,
        float cardRange,
        EntityStats entityStats,
        float movementBudget)
    {
#if UNITY_EDITOR
        if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] Attempting to find path into range. From: {fromPosition}, Target: {targetPosition}, CardRange: {cardRange}, Budget: {movementBudget}");
#endif

        // Try to find a position within range
        var rangePosition = FindRangeAwarePositionForTarget(targetPosition, fromPosition, cardRange);

        if (!rangePosition.HasValue)
        {
#if UNITY_EDITOR
            if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] FAILED - FindRangeAwarePositionForTarget returned null. No valid navmesh position found within {cardRange}m of target");
#endif
            return null;
        }

#if UNITY_EDITOR
        if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] Found valid position on navmesh: {rangePosition.Value}, distance from target: {Vector3.Distance(rangePosition.Value, targetPosition):F2}");
#endif

        // Find path to that position
        var pathData = MovementUtility.FindPath(fromPosition, rangePosition.Value, entityStats);

        if (pathData == null)
        {
#if UNITY_EDITOR
            if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] FAILED - FindPath returned null. Unable to calculate path from {fromPosition} to {rangePosition.Value}");
#endif
            return null;
        }

#if UNITY_EDITOR
        if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] Path found! Cost: {pathData.PathCost:F1}, Budget: {movementBudget}");
#endif

        if (pathData.PathCost > movementBudget)
        {
#if UNITY_EDITOR
            if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] REJECTED - Path cost {pathData.PathCost:F1} exceeds movement budget {movementBudget}");
#endif
            return null;
        }

#if UNITY_EDITOR
        if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindPathIntoRange] SUCCESS - Path to casting position found with cost {pathData.PathCost:F1}");
#endif
        return (pathData, rangePosition.Value);
    }

         /// <summary>
         /// Finds a closer position to get targets in range using a spiral search pattern.
         /// Searches in expanding circles, prioritizing positions toward the target centroid.
         /// Prefers the outermost valid position to encourage ranged units to maintain distance.
         /// Best for multi-target scenarios where broad positional search is beneficial.
         /// CRITICAL: Validates that the final path destination (pathData.End) is still in range,
         /// not just the candidate position, to account for NavMesh snapping.
         /// Returns NavMeshPathData to a closer position, or null if not found.
         /// </summary>
         public static NavMeshPathData FindCloserPositionSpiral(Vector3 currentPos, List<EntityScript> targets, float cardRange, EntityStats entityStats = null)
         {
             if (targets == null || targets.Count == 0)
                 return null;

             float RANGE_TOLERANCE = 0.1f;
             const int anglesPerRing = 8;
             const int maxPositionsToTry = 12;

             // Find the centroid of all targets
             Vector3 targetCentroid = Vector3.zero;
             foreach (var target in targets)
             {
                 if (target != null)
                     targetCentroid += target.transform.position;
             }
             targetCentroid /= Mathf.Max(1, targets.Count);

             // Direction from current position toward targets
             Vector3 directionToTargets = (targetCentroid - currentPos).normalized;

             // Try positions in an expanding spiral toward the targets
             // Start from further distances and expand outward to prefer outermost valid positions
             int positionsTested = 0;
             NavMeshPathData bestPathData = null;
             float bestDistance = float.MaxValue;

             for (float distance = 1f; distance <= cardRange + 2f && positionsTested < maxPositionsToTry; distance += 1f)
             {
                 // Test angles, prioritizing direction toward targets
                 for (int angleIndex = 0; angleIndex < anglesPerRing; angleIndex++)
                 {
                     if (positionsTested >= maxPositionsToTry)
                         break;

                     float angle = (angleIndex * 360f / anglesPerRing) * Mathf.Deg2Rad;
                     Vector3 candidate = currentPos + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * distance;

                     // Bias toward the direction of targets
                     float dotProduct = Vector3.Dot((candidate - currentPos).normalized, directionToTargets);
                     if (dotProduct < 0.3f) // Skip directions away from targets
                         continue;

                     // Snap to navmesh
                     NavMeshHit hit;
                     if (NavMesh.SamplePosition(candidate, out hit, 1f, NavMesh.AllAreas))
                     {
                         candidate = hit.position;

                         // Check if this position gets all targets in range
                         bool allInRange = true;
                         foreach (var target in targets)
                         {
                             if (target == null)
                                 continue;

                             float distToTarget = Vector3.Distance(candidate, target.transform.position);
                             if (distToTarget > cardRange + RANGE_TOLERANCE)
                             {
                                 allInRange = false;
                                 break;
                             }
                         }

                         if (allInRange)
                         {
                             // Try to find path to this position
                             var pathData = MovementUtility.FindPath(currentPos, candidate, entityStats: entityStats);
                             if (pathData != null && pathData.CachedNavMeshPath != null && pathData.CachedNavMeshPath.corners.Length > 0)
                             {
                                 // CRITICAL: Re-validate that the final destination (pathData.End after NavMesh snapping) is still in range
                                 // The path destination might snap to a different NavMesh position, so we must verify it's still valid
                                 bool finalDestinationInRange = true;
                                 foreach (var target in targets)
                                 {
                                     if (target == null)
                                         continue;

                                     float distToTarget = Vector3.Distance(pathData.End, target.transform.position);
                                     if (distToTarget > cardRange + RANGE_TOLERANCE)
                                     {
                                         finalDestinationInRange = false;
    #if UNITY_EDITOR
                                         if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindCloserPositionSpiral] ✗ Final destination {pathData.End} out of range: target at {target.transform.position}, dist {distToTarget:F2} > {cardRange + RANGE_TOLERANCE:F2}");
    #endif
                                         break;
                                     }
                                 }

                                 if (finalDestinationInRange)
                                 {
                                     // Prefer the cheapest (closest) path among all valid positions
                                     if (pathData.PathCost < bestDistance)
                                     {
                                         bestPathData = pathData;
                                         bestDistance = pathData.PathCost;
    #if UNITY_EDITOR
                                         if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindCloserPositionSpiral] ✓ Found valid position at cost {pathData.PathCost}, candidate {candidate}, final destination {pathData.End}");

                                         // Draw debug visualization for the path
                                         DrawPathDebugVisualization(pathData, targetCentroid, cardRange);
    #endif
                                     }
                                 }
                             }
                         }
                     }

                     positionsTested++;
                 }
             }

    #if UNITY_EDITOR
             if (ENABLE_TARGETING_LOGGING && bestPathData == null) Debug.Log($"[FindCloserPositionSpiral] No closer in-range position found after {positionsTested} tests");
    #endif
             return bestPathData;
    }

         /// <summary>
         /// Finds a closer position to get targets in range using a directed approach along the caster-to-target line.
         /// Searches positions along the line between caster and target centroid, preferring the outermost valid position
         /// (furthest from target but still in range) to encourage ranged units to maintain distance.
         /// Most efficient for single-target or clustered multi-target scenarios.
         /// CRITICAL: Validates that the final path destination (pathData.End) is still in range,
         /// not just the candidate position, to account for NavMesh snapping.
         /// Returns NavMeshPathData to a closer position, or null if not found.
         /// </summary>
         public static NavMeshPathData FindCloserPositionDirected(Vector3 currentPos, List<EntityScript> targets, float cardRange, EntityStats entityStats = null)
         {
             if (targets == null || targets.Count == 0)
                 return null;

             float RANGE_TOLERANCE = 0.1f;
             const float SEARCH_STEP = 0.2f;

             // Find the centroid of all targets
             Vector3 targetCentroid = Vector3.zero;
             foreach (var target in targets)
             {
                 if (target != null)
                     targetCentroid += target.transform.position;
             }
             targetCentroid /= Mathf.Max(1, targets.Count);

             // Direction from current position toward targets
             Vector3 directionToTargets = (targetCentroid - currentPos).normalized;
             float distToTargetCentroid = Vector3.Distance(currentPos, targetCentroid);

             // Search along the line from current to target
             // Start from far positions (away from target) and move closer, preferring the outermost valid position
             // This encourages ranged units to maintain distance at the edge of their range
             NavMeshPathData bestPathData = null;
             float bestDistance = float.MaxValue;

             for (float offsetFromTarget = distToTargetCentroid; offsetFromTarget >= 0f; offsetFromTarget -= SEARCH_STEP)
             {
                 // Position along the line, offset backward from the target
                 Vector3 candidate = targetCentroid - (directionToTargets * offsetFromTarget);

                 // Snap to navmesh
                 NavMeshHit hit;
                 if (NavMesh.SamplePosition(candidate, out hit, 1f, NavMesh.AllAreas))
                 {
                     candidate = hit.position;

                     // Check if this position gets all targets in range
                     bool allInRange = true;
                     foreach (var target in targets)
                     {
                         if (target == null)
                             continue;

                         float distToTarget = Vector3.Distance(candidate, target.transform.position);
                         if (distToTarget > cardRange + RANGE_TOLERANCE)
                         {
                             allInRange = false;
                             break;
                         }
                     }

                     if (allInRange)
                     {
                         // Try to find path to this position
                         var pathData = MovementUtility.FindPath(currentPos, candidate, entityStats: entityStats);
                         if (pathData != null && pathData.CachedNavMeshPath != null && pathData.CachedNavMeshPath.corners.Length > 0)
                         {
                             // CRITICAL: Re-validate that the final destination (pathData.End after NavMesh snapping) is still in range
                             // The path destination might snap to a different NavMesh position, so we must verify it's still valid
                             bool finalDestinationInRange = true;
                             foreach (var target in targets)
                             {
                                 if (target == null)
                                     continue;

                                 float distToTarget = Vector3.Distance(pathData.End, target.transform.position);
                                 if (distToTarget > cardRange + RANGE_TOLERANCE)
                                 {
                                     finalDestinationInRange = false;
    #if UNITY_EDITOR
                                     if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindCloserPositionDirected] ✗ Final destination {pathData.End} out of range: target at {target.transform.position}, dist {distToTarget:F2} > {cardRange + RANGE_TOLERANCE:F2}");
    #endif
                                     break;
                                 }
                             }

                             if (finalDestinationInRange)
                             {
                                 // Prefer the outermost valid position (furthest from target but still in range)
                                 if (pathData.PathCost < bestDistance)
                                 {
                                     bestPathData = pathData;
                                     bestDistance = pathData.PathCost;
    #if UNITY_EDITOR
                                     if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindCloserPositionDirected] ✓ Found valid position at cost {pathData.PathCost}, candidate {candidate}, final destination {pathData.End}");

                                     // Draw debug visualization for the path
                                     DrawPathDebugVisualization(pathData, targetCentroid, cardRange);
    #endif
                                 }
                             }
                         }
                     }
            }
                 }

        #if UNITY_EDITOR
                 if (ENABLE_TARGETING_LOGGING) Debug.Log($"[FindCloserPositionDirected] No closer in-range position found along the directed line");
        #endif
                 return bestPathData;
    }

    #endregion

    #region Debug Visualization

    /// <summary>
    /// Draws temporary debug lines to visualize path positions including Start, corners, End, and aim (target centroid).
    /// Helps diagnose pathfinding and range issues.
    /// </summary>
    private static void DrawPathDebugVisualization(NavMeshPathData pathData, Vector3 aimPosition, float cardRange)
    {
        if (pathData?.CachedNavMeshPath == null)
            return;

        const float LINE_HEIGHT = 3f; // Height of vertical debug lines
        const float LINE_DURATION = 5f; // How long the lines persist

        // Draw Start position as a cyan vertical line
        Vector3 startLineTop = pathData.Start + Vector3.up * LINE_HEIGHT;
        Debug.DrawLine(pathData.Start, startLineTop, Color.cyan, LINE_DURATION, false);
        Debug.DrawLine(pathData.Start, pathData.Start + Vector3.up * 0.3f, Color.cyan, LINE_DURATION, false);

        // Draw Path corners as green vertical lines
        if (pathData.CachedNavMeshPath.corners.Length > 0)
        {
            for (int i = 0; i < pathData.CachedNavMeshPath.corners.Length; i++)
            {
                Vector3 corner = pathData.CachedNavMeshPath.corners[i];
                Vector3 cornerLineTop = corner + Vector3.up * LINE_HEIGHT;
                Debug.DrawLine(corner, cornerLineTop, Color.green, LINE_DURATION, false);
            }
        }

        // Draw End position as a yellow vertical line
        Vector3 endLineTop = pathData.End + Vector3.up * LINE_HEIGHT;
        Debug.DrawLine(pathData.End, endLineTop, Color.yellow, LINE_DURATION, false);
        Debug.DrawLine(pathData.End, pathData.End + Vector3.up * 0.5f, Color.yellow, LINE_DURATION, false);

        // Draw Aim (target centroid) position as a red vertical line
        Vector3 aimLineTop = aimPosition + Vector3.up * LINE_HEIGHT;
        Debug.DrawLine(aimPosition, aimLineTop, Color.red, LINE_DURATION, false);

        // Draw a line from End to Aim to show the gap (if any)
        Debug.DrawLine(pathData.End, aimPosition, new Color(1f, 1f, 0f, 0.5f), LINE_DURATION, false); // Yellow-ish

        // Draw a circle at the range distance from aim (for reference)
        DrawRangeCircle(aimPosition, cardRange, Color.magenta, LINE_DURATION);

#if UNITY_EDITOR
        if (ENABLE_TARGETING_LOGGING)
        {
            Debug.Log($"[DebugVisualization] Path visualization drawn:");
            Debug.Log($"  Start (Cyan): {pathData.Start}");
            Debug.Log($"  End (Yellow): {pathData.End}, Distance to Aim: {Vector3.Distance(pathData.End, aimPosition):F2}");
            Debug.Log($"  Aim/Target Centroid (Red): {aimPosition}");
            Debug.Log($"  Card Range Circle (Magenta): radius {cardRange}m from aim");
        }
#endif
    }

    /// <summary>
    /// Draws a circle at the given position to represent the card's range.
    /// </summary>
    private static void DrawRangeCircle(Vector3 center, float radius, Color color, float duration)
    {
        const int segments = 16;
        const float groundHeight = 0.1f;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * 360f * Mathf.Deg2Rad;
            float angle2 = ((i + 1) / (float)segments) * 360f * Mathf.Deg2Rad;

            Vector3 pos1 = center + new Vector3(Mathf.Cos(angle1) * radius, groundHeight, Mathf.Sin(angle1) * radius);
            Vector3 pos2 = center + new Vector3(Mathf.Cos(angle2) * radius, groundHeight, Mathf.Sin(angle2) * radius);

            Debug.DrawLine(pos1, pos2, color, duration, false);
        }
    }

    #endregion
}
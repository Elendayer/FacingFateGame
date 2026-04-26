using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using facingfate;
using UnityEngine.AI;


public static class TargetingUtility
{
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
            CardTargetAffiliation.Ally => targetAff == ownerAff && target != owner,
            CardTargetAffiliation.Enemy => targetAff != ownerAff,
            CardTargetAffiliation.Self => target == owner,
            CardTargetAffiliation.All => true,
            CardTargetAffiliation.AllyNeutral => (targetAff == ownerAff || targetAff == EntityAffiliation.Neutral) && target != owner,
            CardTargetAffiliation.EnemyNeutral => targetAff != ownerAff || targetAff == EntityAffiliation.Neutral,
            CardTargetAffiliation.AllyEnemy => targetAff != EntityAffiliation.Neutral && target != owner,
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

            // Include targets within the ring (between inner and outer radius)
            if (dist < innerRadius || dist > outerRadius)
                continue;

            if (cardData != null && !IsTargetValid(cardData, entity))
                continue;

            results.Add(entity);
        }
        return results;
    }

    /// <summary>
    /// Gets entities within a physics cone (uses raycasts from apex).
    /// </summary>
    public static List<EntityScript> GetEntitiesInPhysicsCone(Vector3 origin, Vector3 direction, float range, float coneAngle, CardData cardData = null)
    {
        List<EntityScript> results = new();
        var allEntities = AllEntitiesCache();

        foreach (var entity in allEntities)
        {
            if (entity == null || !entity.enabled)
                continue;

            if (cardData != null && !IsTargetValid(cardData, entity))
                continue;

            var entityPos = entity.GetComponent<EntityOnMap>();
            if (entityPos == null)
                continue;

            Vector3 toEntity = entityPos.transform.position - origin;
            float distance = toEntity.magnitude;

            if (distance > range)
                continue;

            float angle = Vector3.Angle(direction, toEntity);
            if (angle <= coneAngle * 0.5f)
            {
                // Check line of sight with physics raycast
                if (HasPhysicsLineOfSight(origin, entityPos.transform.position))
                {
                    results.Add(entity);
                }
            }
        }

        return results;
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
                    entities = GetEntitiesInPhysicsRing(aimWorldPos, cardData.Radius, cardData.Area, cardData);
                }
                break;
            case CardTargetingMode.Radius:
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
        DebugVisualization.DrawTargetingData(targetingModeData, cardData.targetingData.cardTargetingMode, cardData, Color.cyan);

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

        // [FindPathIntoRange] Debug: Entry
        Debug.Log($"[FindPathIntoRange] FindRangeAwarePositionForTarget called - OwnerPos: {ownerPos}, TargetPos: {targetPos}, CardRange: {cardRange}, CurrentDist: {currentDist:F2}");

        // Already in range, return the target position
        if (currentDist <= cardRange)
        {
            // [FindPathIntoRange] Debug: Already in range
            Debug.Log($"[FindPathIntoRange] Target already in range! Current distance {currentDist:F2} <= card range {cardRange}");
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
        Debug.Log($"[FindPathIntoRange] Using reference point at: {referencePoint}, (cardRange {cardRange}m from target along owner direction)");

        // First, try the reference point itself
        if (NavMesh.SamplePosition(referencePoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            float distToTarget = Vector3.Distance(hit.position, targetPos);
            positionsTestedCount++;

            Debug.Log($"[FindPathIntoRange] ✓ Valid navmesh position found at iteration {positionsTestedCount}: {hit.position}, dist to target: {distToTarget:F2}");

            if (distToTarget <= cardRange + TOLERANCE)
            {
                Debug.Log($"[FindPathIntoRange] ✓✓ ACCEPTED - Reference point is within tolerance (dist: {distToTarget:F2} <= range+tolerance: {cardRange + TOLERANCE:F2})");
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

                Debug.Log($"[FindPathIntoRange] ✓ Valid navmesh position found at iteration {positionsTestedCount}: {hit.position}, dist to target: {distToTarget:F2}");

                if (distToTarget <= cardRange + TOLERANCE)
                {
                    Debug.Log($"[FindPathIntoRange] ✓✓ ACCEPTED - Position is within tolerance (dist: {distToTarget:F2} <= range+tolerance: {cardRange + TOLERANCE:F2})");
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

                    Debug.Log($"[FindPathIntoRange] ✓ Valid navmesh position found at iteration {positionsTestedCount}: {hit.position}, dist to target: {distToTarget:F2}");

                    if (distToTarget <= cardRange + TOLERANCE)
                    {
                        Debug.Log($"[FindPathIntoRange] ✓✓ ACCEPTED - Position is within tolerance (dist: {distToTarget:F2} <= range+tolerance: {cardRange + TOLERANCE:F2})");
                        return hit.position;
                    }
                }
            }
        }

        // [FindPathIntoRange] Debug: Search exhausted
        Debug.Log($"[FindPathIntoRange] ✗ FAILED - Tested {positionsTestedCount} positions but none were valid navmesh positions within {cardRange}m (+ {TOLERANCE}m tolerance) of target");
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
        // [FindPathIntoRange] Debug: Log entry
        Debug.Log($"[FindPathIntoRange] Attempting to find path into range. From: {fromPosition}, Target: {targetPosition}, CardRange: {cardRange}, Budget: {movementBudget}");

        // Try to find a position within range
        var rangePosition = FindRangeAwarePositionForTarget(targetPosition, fromPosition, cardRange);

        if (!rangePosition.HasValue)
        {
            // [FindPathIntoRange] Debug: Position search failed
            Debug.Log($"[FindPathIntoRange] FAILED - FindRangeAwarePositionForTarget returned null. No valid navmesh position found within {cardRange}m of target");
            return null;
        }

        // [FindPathIntoRange] Debug: Position found
        Debug.Log($"[FindPathIntoRange] Found valid position on navmesh: {rangePosition.Value}, distance from target: {Vector3.Distance(rangePosition.Value, targetPosition):F2}");

        // Find path to that position
        var pathData = MovementUtility.FindPath(fromPosition, rangePosition.Value, entityStats);

        if (pathData == null)
        {
            // [FindPathIntoRange] Debug: Pathfinding failed
            Debug.Log($"[FindPathIntoRange] FAILED - FindPath returned null. Unable to calculate path from {fromPosition} to {rangePosition.Value}");
            return null;
        }

        // [FindPathIntoRange] Debug: Path found, check cost
        Debug.Log($"[FindPathIntoRange] Path found! Cost: {pathData.PathCost:F1}, Budget: {movementBudget}, Cost: {pathData.PathCost:F2}");

        if (pathData.PathCost > movementBudget)
        {
            // [FindPathIntoRange] Debug: Path cost exceeds budget
            Debug.Log($"[FindPathIntoRange] REJECTED - Path cost {pathData.PathCost:F1} exceeds movement budget {movementBudget}");
            return null;
        }

        // [FindPathIntoRange] Debug: Success
        Debug.Log($"[FindPathIntoRange] SUCCESS - Path to casting position found with cost {pathData.PathCost:F1}");
        return (pathData, rangePosition.Value);
    }

    #endregion
}
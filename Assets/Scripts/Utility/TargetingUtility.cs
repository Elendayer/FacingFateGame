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
            entities = entities.FindAll(e => HasPhysicsLineOfSight(castWorldPos, e.transform.position));
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
    #endregion
}

public interface ITargetingMode
{
    List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner);
}

public class TargetingModeData
{
    public Vector3 castingPosition { get; set; }
    public List<EntityScript> targetedEntities { get; set; } = new();
    public List<Vector3> targetedPositions { get; set; } = new();
}

public static class TargetingModeFactory
{
    public static ITargetingMode Create(CardScript card)
    {
        return card.cardData.targetingData.cardTargetingMode switch
        {
            CardTargetingMode.Radius => new RadiusTargetingMode(),
            CardTargetingMode.Ring => new RingTargetingMode(),
            CardTargetingMode.LineSelf => new LineSelfTargetingMode(),
            CardTargetingMode.LineFree => new LineFreeTargetingMode(),
            CardTargetingMode.Cone => new ConeTargetingMode(),
            CardTargetingMode.Select => new SelectionTargetingMode(),
            CardTargetingMode.SelectionUnique => new SelectionTargetingMode(),
            CardTargetingMode.All => new AllTargetingMode(),
            CardTargetingMode.Single => new SingleTargetingMode(),
            _ => new SingleTargetingMode(),
        };
    }
}

public abstract class BaseTargetingMode : ITargetingMode
{
    public abstract List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner);

    protected Vector3 GetWorldPositionFromEntity(EntityScript entity)
    {
        var entityOnMap = entity.GetComponent<EntityOnMap>();
        return entityOnMap != null ? entityOnMap.transform.position : entity.transform.position;
    }
}

/* -----------------------------------------------------------
 * SINGLE TARGET
 * -----------------------------------------------------------*/
public class SingleTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;

            foreach (var t in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(t);
                var distToTarget = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Only include target if within range
                if (distToTarget <= cardRange)
                {
                    results.Add(new TargetingModeData
                    {
                        castingPosition = ownerWorldPos,
                        targetedPositions = new List<Vector3> { targetWorldPos },
                        targetedEntities = new List<EntityScript> { t }
                    });
                }
            }
            return results;
        }
}

/* -----------------------------------------------------------
 * RADIUS
 * -----------------------------------------------------------*/
public class RadiusTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;

            // For each valid target, create a radius effect centered on that target
            foreach (var t in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(t);
                var distToTarget = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Skip targets outside range
                if (distToTarget > cardRange)
                    continue;

                var hitEntities = TargetingUtility.GetEntitiesInPhysicsSphere(targetWorldPos, card.cardData.Radius, card.cardData);

                if (hitEntities.Count > 0)
                {
                    results.Add(new TargetingModeData
                    {
                        castingPosition = ownerWorldPos,
                        targetedPositions = hitEntities.Select(e => e.transform.position).ToList(),
                        targetedEntities = hitEntities
                    });
                }
            }

            // Sort by most targets hit
            results = results.OrderByDescending(r => r.targetedEntities.Count).ToList();

            return results;
        }
}

/* -----------------------------------------------------------
 * RING
 * -----------------------------------------------------------*/
public class RingTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;

            // For each valid target, create a ring effect centered on that target
            foreach (var t in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(t);
                var distToTarget = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Skip targets outside range
                if (distToTarget > cardRange)
                    continue;

                var hitEntities = TargetingUtility.GetEntitiesInPhysicsSphere(targetWorldPos, card.cardData.Radius, card.cardData);

                if (hitEntities.Count > 0)
                {
                    results.Add(new TargetingModeData
                    {
                        castingPosition = ownerWorldPos,
                        targetedPositions = hitEntities.Select(e => e.transform.position).ToList(),
                        targetedEntities = hitEntities
                    });
                }
            }

            // Sort by most targets hit
            results = results.OrderByDescending(r => r.targetedEntities.Count).ToList();

            return results;
        }
}

/* -----------------------------------------------------------
 * LINE SELF
 * -----------------------------------------------------------*/
public class LineSelfTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;

            // For each valid target, generate a line from owner toward that target
            foreach (var target in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(target);
                var direction = (targetWorldPos - ownerWorldPos).normalized;
                var distance = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Skip targets outside range
                if (distance > cardRange)
                    continue;

                var hitEntities = TargetingUtility.GetEntitiesInPhysicsLine(ownerWorldPos, direction, cardRange, card.cardData.Area, card.cardData);

                if (hitEntities.Count > 0)
                {
                    results.Add(new TargetingModeData
                    {
                        castingPosition = ownerWorldPos,
                        targetedPositions = hitEntities.Select(e => e.transform.position).ToList(),
                        targetedEntities = hitEntities
                    });
                }
            }

            // Sort by most targets hit, then by distance of closest target
            results = results
                .OrderByDescending(r => r.targetedEntities.Count)
                .ThenBy(r => r.targetedEntities.Min(e => Vector3.Distance(ownerWorldPos, e.transform.position)))
                .ToList();

            return results;
        }
}

/* -----------------------------------------------------------
 * LINE FREE
 * -----------------------------------------------------------*/
public class LineFreeTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;

            // Generate lines between pairs of valid targets
            for (int i = 0; i < targets.Count; i++)
            {
                for (int j = i + 1; j < targets.Count; j++)
                {
                    var startTarget = targets[i];
                    var endTarget = targets[j];
                    var startWorldPos = GetWorldPositionFromEntity(startTarget);
                    var endWorldPos = GetWorldPositionFromEntity(endTarget);

                    // Both targets must be within range of the owner
                    if (Vector3.Distance(ownerWorldPos, startWorldPos) > cardRange || Vector3.Distance(ownerWorldPos, endWorldPos) > cardRange)
                        continue;

                    var direction = (endWorldPos - startWorldPos).normalized;
                    var distance = Vector3.Distance(startWorldPos, endWorldPos);

                    var hitEntities = TargetingUtility.GetEntitiesInPhysicsLine(startWorldPos, direction, distance, card.cardData.Area, card.cardData);

                    if (hitEntities.Count > 0)
                    {
                        results.Add(new TargetingModeData
                        {
                            castingPosition = ownerWorldPos,
                            targetedPositions = hitEntities.Select(e => e.transform.position).ToList(),
                            targetedEntities = hitEntities
                        });
                    }
                }
            }

            // Sort by most targets hit
            results = results.OrderByDescending(r => r.targetedEntities.Count).ToList();

            return results;
        }
}

/* -----------------------------------------------------------
 * CONE
 * -----------------------------------------------------------*/
public class ConeTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;

            // For each valid target, generate a cone aimed at that target
            foreach (var t in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(t);
                var distToTarget = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Skip targets outside range
                if (distToTarget > cardRange)
                    continue;

                var direction = (targetWorldPos - ownerWorldPos).normalized;
                var hitEntities = TargetingUtility.GetEntitiesInPhysicsCone(ownerWorldPos, direction, cardRange, card.cardData.Area, card.cardData);

                if (hitEntities.Count > 0)
                {
                    results.Add(new TargetingModeData
                    {
                        castingPosition = ownerWorldPos,
                        targetedPositions = hitEntities.Select(e => e.transform.position).ToList(),
                        targetedEntities = hitEntities
                    });
                }
            }

            // Sort by most targets hit, then by distance of closest target
            results = results
                .OrderByDescending(r => r.targetedEntities.Count)
                .ThenBy(r => r.targetedEntities.Min(e => Vector3.Distance(ownerWorldPos, e.transform.position)))
                .ToList();

            return results;
        }
}

/* -----------------------------------------------------------
 * SELECTION
 * -----------------------------------------------------------*/
public class SelectionTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;

            foreach (var t in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(t);
                var distToTarget = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Skip targets outside range
                if (distToTarget > cardRange)
                    continue;

                var hitEntities = TargetingUtility.GetEntitiesInPhysicsSphere(targetWorldPos, card.cardData.Radius, card.cardData);

                results.Add(new TargetingModeData
                {
                    castingPosition = ownerWorldPos,
                    targetedPositions = hitEntities.Select(e => e.transform.position).ToList(),
                    targetedEntities = hitEntities
                });
            }

            // Sort by most targets hit
            results = results.OrderByDescending(r => r.targetedEntities.Count).ToList();

            return results;
        }
}

/* -----------------------------------------------------------
 * ALL
 * -----------------------------------------------------------*/
public class AllTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var allEntities = TargetingUtility.AllEntitiesCache();
            var validEntities = allEntities
                .Where(e => e != null && TargetingUtility.IsTargetValid(card.cardData, e, owner))
            .Take(card.cardData.MaxTarget)
            .ToList();

        var ownerWorldPos = GetWorldPositionFromEntity(owner);

        TargetingModeData result = new TargetingModeData
        {
            castingPosition = ownerWorldPos,
            targetedPositions = validEntities.Select(e => e.transform.position).ToList(),
            targetedEntities = validEntities,
        };

        return new List<TargetingModeData>() { result };
    }
}
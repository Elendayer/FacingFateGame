using facingfate;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;


namespace facingfate
{
    public interface ITargetingMode
    {
        List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner);
    }
    public class TargetingModeData
    {
        // Where the Player should be
        public Vector3 castingPosition { get; set; }

        //Where the Effect should be
        public Vector3 aimPosition { get; set; }

        public List<EntityScript> targetedEntities { get; set; } = new();
        public List<Vector3> targetedPositions { get; set; } = new();
        public bool IsReachable { get; set; } = true;
        /// <summary>
        /// Cached path data for reaching the casting position. Set by targeting modes
        /// when they determine a movement is required to get into range.
        /// </summary>
        public NavMeshPathData pathToRangePosition { get; set; }
    }

    /// <summary>
    /// Factory for creating targeting mode implementations based on card targeting type.
    /// </summary>
    public static class TargetingModeFactory
    {
        public static ITargetingMode Create(CardScript card)
        {
            ITargetingMode mode = null;
            switch (card.cardData.targetingData.cardTargetingMode)
            {
                case CardTargetingMode.Sphere:
                    mode = new RadiusTargetingMode();
                    break;
                case CardTargetingMode.Ring:
                    mode = new RingTargetingMode();
                    break;
                case CardTargetingMode.RingSelf:
                    mode = new RingSelfTargetingMode();
                    break;
                case CardTargetingMode.LineSelf:
                    mode = new LineSelfTargetingMode();
                    break;
                case CardTargetingMode.LineFree:
                    mode = new LineFreeTargetingMode();
                    break;
                case CardTargetingMode.Cone:
                    mode = new ConeTargetingMode();
                    break;
                case CardTargetingMode.Select:
                    mode = new SelectionTargetingMode();
                    break;
                case CardTargetingMode.SelectionUnique:
                    mode = new SelectionTargetingMode();
                    break;
                case CardTargetingMode.All:
                    mode = new AllTargetingMode();
                    break;
                case CardTargetingMode.Single:
                    mode = new SingleTargetingMode();
                    break;
                default:
                    mode = new SingleTargetingMode();
                    break;
            }

            Debug.Log($"[TargetingModeFactory] Created targeting mode: {card.cardData.targetingData.cardTargetingMode}");
            return mode;
        }
    }

    /// <summary>
    /// Base class for all targeting mode implementations.
    /// Provides common functionality for converting entities to targeting data.
    /// </summary>
    public abstract class BaseTargetingMode : ITargetingMode
    {
        public abstract List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner);

        protected Vector3 GetWorldPositionFromEntity(EntityScript entity)
        {
            var entityOnMap = entity.GetComponent<EntityOnMap>();
            return entityOnMap != null ? entityOnMap.transform.position : entity.transform.position;
        }

        /// <summary>
        /// Returns true if the target is out of range. Self-affiliation targets (owner == target)
        /// always pass the range check regardless of card range.
        /// </summary>
        protected bool IsOutOfRange(EntityScript target, EntityScript owner, Vector3 ownerWorldPos, Vector3 targetWorldPos, float cardRange, CardData cardData)
        {
            if (cardData.targetingData.CardTargetAffiliation == CardTargetAffiliation.Self)
                return false;
            return Vector3.Distance(ownerWorldPos, targetWorldPos) > cardRange;
        }

        /// <summary>
        /// Finds a position on the navmesh close to the target entity that is within range.
        /// For entities not on navmesh (off-turn entities), this finds the closest walkable position.
        /// Returns the position and its distance from owner, or null if unreachable.
        /// </summary>
        protected (Vector3 position, float distanceFromOwner) FindRangeAwarePosition(EntityScript entity, EntityScript owner, float cardRange)
        {
            var targetPos = GetWorldPositionFromEntity(entity);
            var ownerPos = GetWorldPositionFromEntity(owner);
            float currentDist = Vector3.Distance(ownerPos, targetPos);

            // Already in range, return the entity position
            if (currentDist <= cardRange)
            {
                return (targetPos, currentDist);
            }

            // Out of range - need to find a position on navmesh within range
            // Search for navmesh position nearest to target but within card range
            float searchRadius = Mathf.Max(cardRange, 5f);
            for (float angle = 0; angle < 360; angle += 30)
            {
                float rad = angle * Mathf.Deg2Rad;
                // Try positions approaching the target at various angles within range
                for (float dist = cardRange * 0.95f; dist > 0; dist -= 0.5f)
                {
                    Vector3 candidate = targetPos - (new Vector3(Mathf.Cos(rad), 0, Mathf.Sin(rad)) * dist);

                    if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                    {
                        float distFromOwner = Vector3.Distance(ownerPos, hit.position);
                        if (distFromOwner <= cardRange)
                        {
                            return (hit.position, distFromOwner);
                        }
                    }
                }
            }

            // Fallback: if no position found, return entity position with out-of-range marker
            return (targetPos, float.MaxValue);
        }
    }

    /// <summary>
    /// Single target targeting mode - each valid target is a separate targeting option.
    /// </summary>
    public class SingleTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;
            var usesVision = card.cardData.targetingData.TargetingUsesVision;

            Debug.Log($"[SingleTargetingMode] Starting evaluation - Valid targets: {targets.Count}, Card range: {cardRange}, Uses vision: {usesVision}");

            foreach (var t in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(t);
                var distToTarget = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Only include target if within range (Self targets always pass)
                if (IsOutOfRange(t, owner, ownerWorldPos, targetWorldPos, cardRange, card.cardData))
                {
                    Debug.Log($"[SingleTargetingMode] Target '{t.name}' rejected: OUT OF RANGE (distance: {distToTarget:F2}, card range: {cardRange:F2})");
                    continue;
                }

                // Check vision if enabled
                if (usesVision && !TargetingUtility.HasPhysicsLineOfSight(ownerWorldPos, targetWorldPos))
                {
                    Debug.Log($"[SingleTargetingMode] Target '{t.name}' rejected: NO LINE OF SIGHT (distance was OK: {distToTarget:F2})");
                    continue;
                }

                Debug.Log($"[SingleTargetingMode] Target '{t.name}' ACCEPTED (distance: {distToTarget:F2}, has LOS: {!usesVision || TargetingUtility.HasPhysicsLineOfSight(ownerWorldPos, targetWorldPos)})");

                results.Add(new TargetingModeData
                {
                    castingPosition = ownerWorldPos,
                    aimPosition = targetWorldPos,
                    targetedPositions = new List<Vector3> { targetWorldPos },
                    targetedEntities = new List<EntityScript> { t },
                    IsReachable = true
                });
            }

            Debug.Log($"[SingleTargetingMode] Final results: {results.Count} valid targeting options");
            return results;
        }
    }

    /// <summary>
    /// Radius targeting mode - creates a sphere effect centered on each valid target.
    /// </summary>
    public class RadiusTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;
            var usesVision = card.cardData.targetingData.TargetingUsesVision;
            var radius = card.cardData.Radius;

            Debug.Log($"[RadiusTargetingMode] Starting evaluation - Valid targets: {targets.Count}, Card range: {cardRange}, Radius: {radius}, Uses vision: {usesVision}");

            // For each valid target, create a radius effect centered on that target
            foreach (var t in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(t);
                var distToTarget = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Skip targets outside range (Self targets always pass)
                if (IsOutOfRange(t, owner, ownerWorldPos, targetWorldPos, cardRange, card.cardData))
                {
                    Debug.Log($"[RadiusTargetingMode] Target '{t.name}' skipped: OUT OF RANGE (distance: {distToTarget:F2})");
                    continue;
                }

                var hitEntities = TargetingUtility.GetEntitiesInPhysicsSphere(targetWorldPos, radius, card.cardData);
                Debug.Log($"[RadiusTargetingMode] Target '{t.name}' at distance {distToTarget:F2}: found {hitEntities.Count} entities in radius sphere");

                // Filter by vision if enabled
                if (usesVision && hitEntities.Count > 0)
                {
                    int beforeVision = hitEntities.Count;
                    hitEntities = hitEntities.FindAll(e => TargetingUtility.HasPhysicsLineOfSight(ownerWorldPos, GetWorldPositionFromEntity(e)));
                    Debug.Log($"[RadiusTargetingMode] Vision filter applied: {beforeVision} -> {hitEntities.Count} entities");
                }

                if (hitEntities.Count > 0)
                {
                    Debug.Log($"[RadiusTargetingMode] Targeting option created with {hitEntities.Count} entities");
                    results.Add(new TargetingModeData
                    {
                        castingPosition = ownerWorldPos,
                        aimPosition = targetWorldPos,
                        targetedPositions = hitEntities.Select(e => GetWorldPositionFromEntity(e)).ToList(),
                        targetedEntities = hitEntities,
                        IsReachable = true
                    });
                }
                else
                {
                    Debug.Log($"[RadiusTargetingMode] Target '{t.name}' rejected: No entities in radius after filtering");
                }
            }

            // Sort by most targets hit
            results = results.OrderByDescending(r => r.targetedEntities.Count).ToList();

            Debug.Log($"[RadiusTargetingMode] Final results: {results.Count} valid targeting options");
            return results;
        }
    }

    /// <summary>
    /// Ring targeting mode - creates a ring effect (hollow circle) centered on each valid target.
    /// </summary>
    public class RingTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);

            CardData cardData = card.cardData;


            // For each valid target, create a ring effect centered on that target
            foreach (var t in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(t);
                var distToTarget = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Skip targets outside range (Self targets always pass)
                if (IsOutOfRange(t, owner, ownerWorldPos, targetWorldPos, cardData.Range, cardData))
                {
                    continue;
                }

                var hitEntities = TargetingUtility.GetEntitiesInPhysicsRing(targetWorldPos, cardData.Radius, cardData.Radius + cardData.Area, cardData);

                // Filter by vision if enabled
                if (cardData.targetingData.TargetingUsesVision && hitEntities.Count > 0)
                {
                    int beforeVision = hitEntities.Count;
                    hitEntities = hitEntities.FindAll(e => TargetingUtility.HasPhysicsLineOfSight(ownerWorldPos, GetWorldPositionFromEntity(e)));
                }

                if (hitEntities.Count > 0)
                {
                    results.Add(new TargetingModeData
                    {
                        castingPosition = ownerWorldPos,
                        aimPosition = targetWorldPos,
                        targetedPositions = hitEntities.Select(e => GetWorldPositionFromEntity(e)).ToList(),
                        targetedEntities = hitEntities,
                        IsReachable = true
                    });
                }
            }

            // Sort by most targets hit
            results = results.OrderByDescending(r => r.targetedEntities.Count).ToList();

            Debug.Log($"[RingTargetingMode] Final results: {results.Count} valid targeting options");
            return results;
        }
    }

    /// <summary>
    /// Ring self targeting mode - creates a ring effect (hollow circle) centered on the caster.
    /// Unlike Ring mode, this can only be cast at the caster's position.
    /// </summary>
    public class RingSelfTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);

            CardData cardData = card.cardData;

            Debug.Log($"[RingSelfTargetingMode] Starting evaluation at caster position: {ownerWorldPos}, Card range: {cardData.Range}, Radius: {cardData.Radius}, Area: {cardData.Area}");

            // Get all entities in the ring centered on the caster
            var hitEntities = TargetingUtility.GetEntitiesInPhysicsRing(ownerWorldPos, cardData.Radius, cardData.Radius + cardData.Area, cardData);

            // Filter by vision if enabled
            if (cardData.targetingData.TargetingUsesVision && hitEntities.Count > 0)
            {
                int beforeVision = hitEntities.Count;
                hitEntities = hitEntities.FindAll(e => TargetingUtility.HasPhysicsLineOfSight(ownerWorldPos, GetWorldPositionFromEntity(e)));
                Debug.Log($"[RingSelfTargetingMode] Vision filter: {beforeVision} entities -> {hitEntities.Count} after vision check");
            }

            Debug.Log($"[RingSelfTargetingMode] Found {hitEntities.Count} entities in ring");

            if (hitEntities.Count > 0)
            {
                results.Add(new TargetingModeData
                {
                    castingPosition = ownerWorldPos,
                    aimPosition = ownerWorldPos,
                    targetedPositions = hitEntities.Select(e => GetWorldPositionFromEntity(e)).ToList(),
                    targetedEntities = hitEntities,
                    IsReachable = true
                });
            }

            Debug.Log($"[RingSelfTargetingMode] Final results: {results.Count} valid targeting options");
            return results;
        }
    }

    /// <summary>
    /// Line self targeting mode - creates a line effect from the caster toward each valid target.
    /// </summary>
    public class LineSelfTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;
            var usesVision = card.cardData.targetingData.TargetingUsesVision;
            var area = card.cardData.Area;

            Debug.Log($"[LineSelfTargetingMode] Starting evaluation - Valid targets: {targets.Count}, Card range: {cardRange}, Area: {area}, Uses vision: {usesVision}");

            // For each valid target, generate a line from owner toward that target
            foreach (var target in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(target);
                var direction = (targetWorldPos - ownerWorldPos).normalized;
                var distance = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Skip targets outside range (Self targets always pass)
                if (IsOutOfRange(target, owner, ownerWorldPos, targetWorldPos, cardRange, card.cardData))
                {
                    Debug.Log($"[LineSelfTargetingMode] Target '{target.name}' skipped: OUT OF RANGE (distance: {distance:F2})");
                    continue;
                }

                var hitEntities = TargetingUtility.GetEntitiesInPhysicsLine(ownerWorldPos, direction, cardRange, area, card.cardData);
                Debug.Log($"[LineSelfTargetingMode] Target '{target.name}' at distance {distance:F2}: found {hitEntities.Count} entities in line");

                // Filter by vision if enabled
                if (usesVision && hitEntities.Count > 0)
                {
                    int beforeVision = hitEntities.Count;
                    hitEntities = hitEntities.FindAll(e => TargetingUtility.HasPhysicsLineOfSight(ownerWorldPos, GetWorldPositionFromEntity(e)));
                    Debug.Log($"[LineSelfTargetingMode] Vision filter applied: {beforeVision} -> {hitEntities.Count} entities");
                }

                if (hitEntities.Count > 0)
                {
                    Debug.Log($"[LineSelfTargetingMode] Targeting option created with {hitEntities.Count} entities");
                    results.Add(new TargetingModeData
                    {
                        castingPosition = ownerWorldPos,
                        aimPosition = targetWorldPos,
                        targetedPositions = hitEntities.Select(e => GetWorldPositionFromEntity(e)).ToList(),
                        targetedEntities = hitEntities,
                        IsReachable = true
                    });
                }
                else
                {
                    Debug.Log($"[LineSelfTargetingMode] Target '{target.name}' rejected: No entities in line after filtering");
                }
            }

            // Sort by most targets hit, then by distance of closest target
            results = results
                .OrderByDescending(r => r.targetedEntities.Count)
                .ThenBy(r => r.targetedEntities.Min(e => Vector3.Distance(ownerWorldPos, GetWorldPositionFromEntity(e))))
                .ToList();

            Debug.Log($"[LineSelfTargetingMode] Final results: {results.Count} valid targeting options");
            return results;
        }
    }

    /// <summary>
    /// Line free targeting mode - creates line effects between pairs of valid targets.
    /// </summary>
    public class LineFreeTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;
            var usesVision = card.cardData.targetingData.TargetingUsesVision;
            var area = card.cardData.Area;

            Debug.Log($"[LineFreeTargetingMode] Starting evaluation - Valid targets: {targets.Count}, Card range: {cardRange}, Area: {area}, Uses vision: {usesVision}");

            // Generate lines between pairs of valid targets
            for (int i = 0; i < targets.Count; i++)
            {
                for (int j = i + 1; j < targets.Count; j++)
                {
                    var startTarget = targets[i];
                    var endTarget = targets[j];
                    var startWorldPos = GetWorldPositionFromEntity(startTarget);
                    var endWorldPos = GetWorldPositionFromEntity(endTarget);

                    // Both targets must be within range of the owner (Self targets always pass)
                    float distStart = Vector3.Distance(ownerWorldPos, startWorldPos);
                    float distEnd = Vector3.Distance(ownerWorldPos, endWorldPos);

                    if (IsOutOfRange(startTarget, owner, ownerWorldPos, startWorldPos, cardRange, card.cardData) ||
                        IsOutOfRange(endTarget, owner, ownerWorldPos, endWorldPos, cardRange, card.cardData))
                    {
                        Debug.Log($"[LineFreeTargetingMode] Line '{startTarget.name}'-'{endTarget.name}' skipped: OUT OF RANGE (distances: {distStart:F2}, {distEnd:F2})");
                        continue;
                    }

                    var direction = (endWorldPos - startWorldPos).normalized;
                    var distance = Vector3.Distance(startWorldPos, endWorldPos);

                    var hitEntities = TargetingUtility.GetEntitiesInPhysicsLine(startWorldPos, direction, distance, area, card.cardData);
                    Debug.Log($"[LineFreeTargetingMode] Line '{startTarget.name}'-'{endTarget.name}': found {hitEntities.Count} entities in line");

                    // Filter by vision if enabled
                    if (usesVision && hitEntities.Count > 0)
                    {
                        int beforeVision = hitEntities.Count;
                        hitEntities = hitEntities.FindAll(e => TargetingUtility.HasPhysicsLineOfSight(ownerWorldPos, GetWorldPositionFromEntity(e)));
                        Debug.Log($"[LineFreeTargetingMode] Vision filter applied: {beforeVision} -> {hitEntities.Count} entities");
                    }

                    if (hitEntities.Count > 0)
                    {
                        Debug.Log($"[LineFreeTargetingMode] Targeting option created with {hitEntities.Count} entities");
                        results.Add(new TargetingModeData
                        {
                            castingPosition = ownerWorldPos,
                            aimPosition = startWorldPos,
                            targetedPositions = hitEntities.Select(e => GetWorldPositionFromEntity(e)).ToList(),
                            targetedEntities = hitEntities,
                            IsReachable = true
                        });
                    }
                    else
                    {
                        Debug.Log($"[LineFreeTargetingMode] Line '{startTarget.name}'-'{endTarget.name}' rejected: No entities in line after filtering");
                    }
                }
            }

            // Sort by most targets hit
            results = results.OrderByDescending(r => r.targetedEntities.Count).ToList();

            Debug.Log($"[LineFreeTargetingMode] Final results: {results.Count} valid targeting options");
            return results;
        }
    }

    /// <summary>
    /// Cone targeting mode - creates a cone effect aimed at each valid target.
    /// </summary>
    public class ConeTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;
            var usesVision = card.cardData.targetingData.TargetingUsesVision;
            var coneAngle = card.cardData.Area;

            Debug.Log($"[ConeTargetingMode] Starting evaluation - Valid targets: {targets.Count}, Card range: {cardRange}, Cone angle: {coneAngle}, Uses vision: {usesVision}");

            // For each valid target, generate a cone aimed at that target
            foreach (var t in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(t);
                var distToTarget = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Skip targets outside range (Self targets always pass)
                if (IsOutOfRange(t, owner, ownerWorldPos, targetWorldPos, cardRange, card.cardData))
                {
                    Debug.Log($"[ConeTargetingMode] Target '{t.name}' skipped: OUT OF RANGE (distance: {distToTarget:F2})");
                    continue;
                }

                var direction = (targetWorldPos - ownerWorldPos).normalized;
                var hitEntities = TargetingUtility.GetEntitiesInPhysicsCone(ownerWorldPos, direction, cardRange, coneAngle, card.cardData);
                Debug.Log($"[ConeTargetingMode] Target '{t.name}' at distance {distToTarget:F2}: found {hitEntities.Count} entities in cone");

                // Filter by vision if enabled
                if (usesVision && hitEntities.Count > 0)
                {
                    int beforeVision = hitEntities.Count;
                    hitEntities = hitEntities.FindAll(e => TargetingUtility.HasPhysicsLineOfSight(ownerWorldPos, GetWorldPositionFromEntity(e)));
                    Debug.Log($"[ConeTargetingMode] Vision filter applied: {beforeVision} -> {hitEntities.Count} entities");
                }

                if (hitEntities.Count > 0)
                {
                    Debug.Log($"[ConeTargetingMode] Targeting option created with {hitEntities.Count} entities");
                    results.Add(new TargetingModeData
                    {
                        castingPosition = ownerWorldPos,
                        aimPosition = targetWorldPos,
                        targetedPositions = hitEntities.Select(e => GetWorldPositionFromEntity(e)).ToList(),
                        targetedEntities = hitEntities,
                        IsReachable = true
                    });
                }
                else
                {
                    Debug.Log($"[ConeTargetingMode] Target '{t.name}' rejected: No entities in cone after filtering");
                }
            }

            // Sort by most targets hit, then by distance of closest target
            results = results
                .OrderByDescending(r => r.targetedEntities.Count)
                .ThenBy(r => r.targetedEntities.Min(e => Vector3.Distance(ownerWorldPos, GetWorldPositionFromEntity(e))))
                .ToList();

            Debug.Log($"[ConeTargetingMode] Final results: {results.Count} valid targeting options");
            return results;
        }
    }

    /// <summary>
    /// Selection targeting mode - allows selecting individual targets with radius effects.
    /// </summary>
    public class SelectionTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var targets = TargetingUtility.GetValidTargets(card.cardData, owner);
            var results = new List<TargetingModeData>();
            var ownerWorldPos = GetWorldPositionFromEntity(owner);
            var cardRange = card.cardData.Range;
            var usesVision = card.cardData.targetingData.TargetingUsesVision;
            var radius = card.cardData.Radius;

            Debug.Log($"[SelectionTargetingMode] Starting evaluation - Valid targets: {targets.Count}, Card range: {cardRange}, Radius: {radius}, Uses vision: {usesVision}");

            foreach (var t in targets)
            {
                var targetWorldPos = GetWorldPositionFromEntity(t);
                var distToTarget = Vector3.Distance(ownerWorldPos, targetWorldPos);

                // Skip targets outside range (Self targets always pass)
                if (IsOutOfRange(t, owner, ownerWorldPos, targetWorldPos, cardRange, card.cardData))
                {
                    Debug.Log($"[SelectionTargetingMode] Target '{t.name}' skipped: OUT OF RANGE (distance: {distToTarget:F2})");
                    continue;
                }

                // Check vision if enabled
                if (usesVision && !TargetingUtility.HasPhysicsLineOfSight(ownerWorldPos, targetWorldPos))
                {
                    Debug.Log($"[SelectionTargetingMode] Target '{t.name}' rejected: NO LINE OF SIGHT");
                    continue;
                }

                var hitEntities = TargetingUtility.GetEntitiesInPhysicsSphere(targetWorldPos, radius, card.cardData);
                Debug.Log($"[SelectionTargetingMode] Target '{t.name}' at distance {distToTarget:F2}: found {hitEntities.Count} entities in selection radius");

                // Filter hit entities by vision if enabled (for AoE part)
                if (usesVision && hitEntities.Count > 0)
                {
                    int beforeVision = hitEntities.Count;
                    hitEntities = hitEntities.FindAll(e => TargetingUtility.HasPhysicsLineOfSight(ownerWorldPos, GetWorldPositionFromEntity(e)));
                    Debug.Log($"[SelectionTargetingMode] Vision filter applied: {beforeVision} -> {hitEntities.Count} entities");
                }

                Debug.Log($"[SelectionTargetingMode] Targeting option created with {hitEntities.Count} entities for selection");
                results.Add(new TargetingModeData
                {
                    castingPosition = ownerWorldPos,
                    aimPosition = targetWorldPos,
                    targetedPositions = hitEntities.Select(e => GetWorldPositionFromEntity(e)).ToList(),
                    targetedEntities = hitEntities,
                    IsReachable = true
                });
            }

            // Sort by most targets hit
            results = results.OrderByDescending(r => r.targetedEntities.Count).ToList();

            Debug.Log($"[SelectionTargetingMode] Final results: {results.Count} valid targeting options");
            return results;
        }
    }

    /// <summary>
    /// All targeting mode - targets all valid entities regardless of range.
    /// </summary>
    public class AllTargetingMode : BaseTargetingMode
    {
        public override List<TargetingModeData> GetTargetingData(CardScript card, EntityScript owner)
        {
            var allEntities = TargetingUtility.AllEntitiesCache();
            Debug.Log($"[AllTargetingMode] Total entities in scene: {allEntities.Count}");

            var validEntities = allEntities
                .Where(e => e != null && TargetingUtility.IsTargetValid(card.cardData, e, owner))
                .ToList();

            Debug.Log($"[AllTargetingMode] Valid targets (affiliation/type check): {validEntities.Count}");

            // Filter by vision if enabled
            var usesVision = card.cardData.targetingData.TargetingUsesVision;
            if (usesVision)
            {
                var ownerWorldPos = GetWorldPositionFromEntity(owner);
                int beforeVision = validEntities.Count;
                validEntities = validEntities.FindAll(e => TargetingUtility.HasPhysicsLineOfSight(ownerWorldPos, GetWorldPositionFromEntity(e))).ToList();
                Debug.Log($"[AllTargetingMode] After vision filter: {beforeVision} -> {validEntities.Count} entities");
            }

            // Apply max target limit
            if (validEntities.Count > card.cardData.MaxTarget)
            {
                Debug.Log($"[AllTargetingMode] Applying max target limit: {validEntities.Count} -> {card.cardData.MaxTarget}");
                validEntities = validEntities.Take(card.cardData.MaxTarget).ToList();
            }

            var ownerPos = GetWorldPositionFromEntity(owner);

            TargetingModeData result = new TargetingModeData
            {
                castingPosition = ownerPos,
                aimPosition = ownerPos,
                targetedPositions = validEntities.Select(e => GetWorldPositionFromEntity(e)).ToList(),
                targetedEntities = validEntities,
                IsReachable = true
            };

            Debug.Log($"[AllTargetingMode] Final result: {result.targetedEntities.Count} total entities in targeting");
            return new List<TargetingModeData>() { result };
        }
    }
}
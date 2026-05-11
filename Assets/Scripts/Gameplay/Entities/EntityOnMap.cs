using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages an entity's position on the NavMesh.
///
/// STATE MACHINE
/// ─────────────
///   IDLE   : NavMeshObstacle ON (carving) | NavMeshAgent OFF
///   MOVING : NavMeshObstacle OFF          | NavMeshAgent ON
///
/// While IDLE the entity carves a hole in the NavMesh so all other
/// agents and path previews naturally route *around* it.
/// While MOVING the hole is gone and the agent steers freely.
/// </summary>

namespace facingfate
{
    public class EntityOnMap : MonoBehaviour
    {
        [SerializeField] private float jumpDuration = 0.35f;
        [SerializeField] private float jumpHeight = 1.5f;

        private NavMeshAgent navMeshAgent;
        private NavMeshObstacle navMeshObstacle;
        private Coroutine moveRoutine;

        /// <summary>World-space position (replaces old Vector3Int currentCell).</summary>
        public Vector3 currentPosition => transform.position;

        // ─────────────────────────────────────────────────────────────────
        // Startup
        // ─────────────────────────────────────────────────────────────────

        public void Startup()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();

            navMeshObstacle = GetComponent<NavMeshObstacle>();
            if (navMeshObstacle == null)
                navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();

            // Capsule matches the agent's own footprint
            navMeshObstacle.shape = NavMeshObstacleShape.Capsule;
            navMeshObstacle.center = Vector3.zero;
            navMeshObstacle.radius = navMeshAgent != null ? navMeshAgent.radius : 0.15f;
            navMeshObstacle.height = 2f;
            navMeshObstacle.carving = true;

            if (navMeshAgent != null)
            {
                // Random priority prevents two agents deadlocking at the same spot
                navMeshAgent.avoidancePriority = Random.Range(30, 70);

                navMeshAgent.stoppingDistance = navMeshAgent.radius * 0.5f;
            }

            SetIdleState();
        }

        // ─────────────────────────────────────────────────────────────────
        // State switching
        // ─────────────────────────────────────────────────────────────────

        /// <summary>Obstacle ON, Agent OFF — entity carves the NavMesh.</summary>
        private void SetIdleState()
        {
            if (navMeshAgent != null) navMeshAgent.enabled = false;
            if (navMeshObstacle != null) navMeshObstacle.enabled = true;
            //Debug.Log($"[EntityOnMap] {name}: SetIdleState called (Obstacle ON, Agent OFF)");
        }

        /// <summary>Disables the NavMeshObstacle for dragging interaction.</summary>
        public void DisableObstacleForDrag()
        {
            if (navMeshObstacle != null) navMeshObstacle.enabled = false;
        }

        /// <summary>Re-enables the NavMeshObstacle after dragging completes.</summary>
        public void EnableObstacleAfterDrag()
        {
            if (navMeshObstacle != null) navMeshObstacle.enabled = true;
        }

        // ─────────────────────────────────────────────────────────────────
        // Public API
        // ─────────────────────────────────────────────────────────────────

        public void TeleportTo(Vector3 position)
        {
            // Brief agent mode just to call Warp, then back to idle
            if (navMeshObstacle != null) navMeshObstacle.enabled = false;
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = true;
                if (NavMesh.SamplePosition(position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                    navMeshAgent.Warp(hit.position);
                navMeshAgent.enabled = false;
            }
            if (navMeshObstacle != null) navMeshObstacle.enabled = true;
        }

        public IEnumerator StartJumpRoutine(Vector3 target)
        {
            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(JumpTo(target));
            yield return moveRoutine;
        }

        public IEnumerator StartMoveRoutineWithPath(NavMeshPathData pathData)
        {
            if (pathData?.CachedNavMeshPath == null || pathData.CachedNavMeshPath.corners.Length == 0) 
            {
                //Debug.LogWarning($"[EntityOnMap] {name}: StartMoveRoutineWithPath called with invalid path data");
                yield break;
            }

            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(FollowPath(pathData));
            yield return moveRoutine;
        }

        // ─────────────────────────────────────────────────────────────────
        // Movement coroutine
        // ─────────────────────────────────────────────────────────────────

        private IEnumerator FollowPath(NavMeshPathData pathData)
        {
            // ── MOVING state: disable obstacle so the carved hole closes ──
            if (navMeshObstacle != null) 
            {
                navMeshObstacle.enabled = false;
                //Debug.Log($"[EntityOnMap] {name}: State → MOVING (Obstacle OFF, carving hole closes)");
            }

            // Two frames for the NavMesh to finish rebuilding
            yield return null;
            yield return null;

            if (navMeshAgent == null) 
            { 
                Debug.LogWarning($"[EntityOnMap] {name}: NavMeshAgent is null, reverting to IDLE");
                SetIdleState(); 
                yield break; 
            }

            navMeshAgent.enabled = true;
            navMeshAgent.velocity = Vector3.zero;
            //Debug.Log($"[EntityOnMap] {name}: NavMeshAgent enabled, velocity reset. Current position: {navMeshAgent.transform.position}");

            // CRITICAL: Snap the agent to the NavMesh BEFORE assigning any path
            // The agent must be on a valid NavMesh position for the path to be accepted
            if (!TrySnapToNavMeshAggressive())
            {
                Debug.LogError($"[EntityOnMap] {name}: Failed to snap agent to NavMesh, reverting to IDLE. Current pos: {transform.position}");
                SetIdleState(); 
                yield break; 
            }

            if (!navMeshAgent.isOnNavMesh) 
            { 
                Debug.LogError($"[EntityOnMap] {name}: Agent not on NavMesh after aggressive snap, reverting to IDLE. isOnNavMesh={navMeshAgent.isOnNavMesh}");
                SetIdleState(); 
                yield break; 
            }

            // Get the current position (snapped) and use it as the actual start for path calculation
            Vector3 actualStart = navMeshAgent.transform.position;
            //Debug.Log($"[EntityOnMap] {name}: Using actual position as path start: {actualStart}");

            //Debug.Log($"[EntityOnMap] {name}: Agent successfully snapped to NavMesh. Position: {navMeshAgent.transform.position}, isOnNavMesh={navMeshAgent.isOnNavMesh}");

            // Disable auto-replanning: prevents the agent changing direction
            // mid-move when another entity switches obstacle state
            navMeshAgent.autoRepath = false;

            // CRITICAL: For planned card casting, the NPC must reach the exact endpoint to be in range.
            // Store the original stopping distance and set it to near-zero for precise positioning.
            float originalStoppingDistance = navMeshAgent.stoppingDistance;
            navMeshAgent.stoppingDistance = 0.01f; // Near-zero to ensure exact endpoint reach

            // CRITICAL FIX: The cached path may be stale because the NavMesh was rebuilt
            // after the obstacle was disabled. Recalculate the path with the current NavMesh state.
            // Use the actual snapped position (not the stale pathData.Start) to avoid teleporting.
            Vector3 pathEnd = pathData.End;

            NavMeshPath freshPath = new NavMeshPath();
            bool pathCalculated = NavMesh.CalculatePath(actualStart, pathEnd, NavMesh.AllAreas, freshPath);

            // CRITICAL FIX: Accept both complete and partial paths
            // Partial paths are valid when chasing enemies (target is blocked by an obstacle)
            // The agent will move to the last reachable corner, which is enough for card range
            if (!pathCalculated || (freshPath.status != NavMeshPathStatus.PathComplete && freshPath.status != NavMeshPathStatus.PathPartial))
            {
                //Debug.LogError($"[EntityOnMap] {name}: Failed to calculate path from {actualStart} to {pathEnd}! pathCalculated={pathCalculated}, status={freshPath.status}. Reverting to IDLE.");
                SetIdleState(); 
                yield break; 
            }

            // Log if we're using a partial path (for diagnostics)
            if (freshPath.status == NavMeshPathStatus.PathPartial)
            {
                Debug.Log($"[EntityOnMap] {name}: Using partial path (target blocked). Will reach corner at {freshPath.corners[freshPath.corners.Length - 1]}");
            }

            //Debug.Log($"[EntityOnMap] {name}: Fresh path calculated with {freshPath.corners.Length} corners (cached had {pathData.CachedNavMeshPath.corners.Length})");

            // Assign the freshly calculated path
            navMeshAgent.path = freshPath;
            //Debug.Log($"[EntityOnMap] {name}: Fresh path assigned, destination: {pathData.End}");

            // CRITICAL: Give NavMeshAgent several frames to process the path assignment
            // If we check hasPath immediately, it may still be false even though the path is valid
            yield return null;
            yield return null;
            yield return null;
            yield return null;

            //Debug.Log($"[EntityOnMap] {name}: After path settling - hasPath: {navMeshAgent.hasPath}, remainingDistance: {navMeshAgent.remainingDistance}, stoppingDistance: {navMeshAgent.stoppingDistance}");

            if (!navMeshAgent.hasPath)
            {
                //Debug.LogError($"[EntityOnMap] {name}: Path failed to set on NavMeshAgent! Reverting to IDLE. Path status: pathPending={navMeshAgent.pathPending}, fresh path corners={freshPath.corners.Length}");
                SetIdleState(); 
                yield break; 
            }

            float elapsed = 0f;
            int frameCount = 0;

            //Debug.Log($"[EntityOnMap] {name}: Starting movement loop - remainingDistance: {navMeshAgent.remainingDistance}, stoppingDistance: {navMeshAgent.stoppingDistance}");

            while (navMeshAgent.hasPath &&
                   navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            {
                if ((elapsed += Time.deltaTime) > 15f)
                {
                    Debug.LogWarning($"[EntityOnMap] {name}: movement timed out after {frameCount} frames.");
                    break;
                }
                frameCount++;

                yield return null;
            }

            float finalDistance = Vector3.Distance(navMeshAgent.transform.position, pathData.End);
            if (finalDistance < 0.5f)
            {
                Debug.Log($"[EntityOnMap] {name}: Movement complete - Reached endpoint! Distance to end: {finalDistance:F3}m (traveled {frameCount} frames). Final position: {navMeshAgent.transform.position}");
            }
            else
            {
                Debug.LogWarning($"[EntityOnMap] {name}: Movement stopped short of endpoint! Distance to end: {finalDistance:F3}m (traveled {frameCount} frames). Final position: {navMeshAgent.transform.position}");
            }

            // ── Back to IDLE state ────────────────────────────────────────
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.autoRepath = true;
            navMeshAgent.stoppingDistance = originalStoppingDistance; // Restore original stopping distance
            navMeshAgent.ResetPath();
            navMeshAgent.enabled = false;
            //Debug.Log($"[EntityOnMap] {name}: NavMeshAgent disabled");

            yield return null; // let position settle before re-carving
            if (navMeshObstacle != null) 
            {
                navMeshObstacle.enabled = true;
                //Debug.Log($"[EntityOnMap] {name}: State → IDLE (Obstacle ON, entity re-carves NavMesh). Final position: {transform.position}");
            }

            moveRoutine = null;
        }

        // ─────────────────────────────────────────────────────────────────
        // Jump coroutine
        // ─────────────────────────────────────────────────────────────────

        private IEnumerator JumpTo(Vector3 target)
        {
            if (navMeshObstacle != null) navMeshObstacle.enabled = false;
            yield return null;
            yield return null;

            if (navMeshAgent == null) { SetIdleState(); yield break; }

            navMeshAgent.enabled = true;
            navMeshAgent.velocity = Vector3.zero;
            TrySnapToNavMesh();
            if (!navMeshAgent.isOnNavMesh) { SetIdleState(); yield break; }

            Vector3 start = transform.position;
            if (!NavMesh.SamplePosition(target, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                hit.position = target;

            for (float t = 0f; t < jumpDuration; t += Time.deltaTime)
            {
                float n = Mathf.Clamp01(t / jumpDuration);
                Vector3 p = Vector3.Lerp(start, hit.position, n);
                p.y += jumpHeight * 4f * n * (1f - n);
                navMeshAgent.Warp(p);
                yield return null;
            }

            navMeshAgent.Warp(hit.position);
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.ResetPath();
            navMeshAgent.enabled = false;

            yield return null;
            if (navMeshObstacle != null) navMeshObstacle.enabled = true;

            moveRoutine = null;
        }

        // ─────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────

        private void TrySnapToNavMesh()
        {
            if (navMeshAgent == null || navMeshAgent.isOnNavMesh) return;
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit.position);
                //Debug.Log($"[EntityOnMap] Snapped {name} to NavMesh at {hit.position}");
            }
            else
                Debug.LogWarning($"[EntityOnMap] Could not snap {name} near {transform.position}");
        }

        /// <summary>
        /// Aggressively snaps the agent to the NavMesh with multiple attempts and verification.
        /// Returns true if the agent ends up on the NavMesh, false otherwise.
        /// </summary>
        private bool TrySnapToNavMeshAggressive()
        {
            if (navMeshAgent == null)
            {
                Debug.LogError($"[EntityOnMap] {name}: NavMeshAgent is null in TrySnapToNavMeshAggressive");
                return false;
            }

            Vector3 startPos = transform.position;
            //Debug.Log($"[EntityOnMap] {name}: Starting aggressive snap from {startPos}, isOnNavMesh={navMeshAgent.isOnNavMesh}");

            // Try 1: Direct sample at current position with larger search radius
            if (NavMesh.SamplePosition(startPos, out NavMeshHit hit1, 5f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit1.position);
                //Debug.Log($"[EntityOnMap] {name}: Attempt 1 - Warped to {hit1.position}, isOnNavMesh={navMeshAgent.isOnNavMesh}");
                if (navMeshAgent.isOnNavMesh)
                    return true;
            }

            // Try 2: Sample at a slightly raised position (common issue with Y=0)
            Vector3 raisedPos = startPos + Vector3.up * 0.5f;
            if (NavMesh.SamplePosition(raisedPos, out NavMeshHit hit2, 5f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit2.position);
                //Debug.Log($"[EntityOnMap] {name}: Attempt 2 - Warped to raised position {hit2.position}, isOnNavMesh={navMeshAgent.isOnNavMesh}");
                if (navMeshAgent.isOnNavMesh)
                    return true;
            }

            // Try 3: Sample at the path's destination if available (entities might need to snap to target area)
            if (NavMesh.SamplePosition(startPos + Vector3.forward * 2f, out NavMeshHit hit3, 5f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit3.position);
                //Debug.Log($"[EntityOnMap] {name}: Attempt 3 - Warped to forward position {hit3.position}, isOnNavMesh={navMeshAgent.isOnNavMesh}");
                if (navMeshAgent.isOnNavMesh)
                    return true;
            }

            // All attempts failed
            Debug.LogError($"[EntityOnMap] {name}: All snap attempts failed. Original pos: {startPos}, Final isOnNavMesh: {navMeshAgent.isOnNavMesh}");
            return false;
        }
    }
}
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
            navMeshObstacle.radius = navMeshAgent != null ? navMeshAgent.radius : 0.4f;
            navMeshObstacle.height = 2f;
            navMeshObstacle.carving = true;

            if (navMeshAgent != null)
            {
                // Random priority prevents two agents deadlocking at the same spot
                navMeshAgent.avoidancePriority = Random.Range(30, 70);

                if (navMeshAgent.stoppingDistance < 0.05f)
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

        public IEnumerator StartMoveRoutine(Vector3 target)
        {
            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(FollowPath(target));
            yield return moveRoutine;
        }

        public IEnumerator StartMoveRoutineWithPath(NavMeshPathData pathData)
        {
            if (pathData?.CachedNavMeshPath == null || pathData.CachedNavMeshPath.corners.Length == 0) yield break;

            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(FollowPath(pathData.End));
            yield return moveRoutine;
        }

        // ─────────────────────────────────────────────────────────────────
        // Movement coroutine
        // ─────────────────────────────────────────────────────────────────

        private IEnumerator FollowPath(Vector3 target)
        {
            // ── MOVING state: disable obstacle so the carved hole closes ──
            if (navMeshObstacle != null) navMeshObstacle.enabled = false;

            // Two frames for the NavMesh to finish rebuilding
            yield return null;
            yield return null;

            if (navMeshAgent == null) { SetIdleState(); yield break; }

            navMeshAgent.enabled = true;
            navMeshAgent.velocity = Vector3.zero;

            TrySnapToNavMesh();
            if (!navMeshAgent.isOnNavMesh) { SetIdleState(); yield break; }

            // Always calculate a fresh path here.
            // The cached PathData path was built during drag-preview; by now
            // other entities may have moved and the NavMesh has changed.
            var fresh = new NavMeshPath();
            bool ok = navMeshAgent.CalculatePath(target, fresh) &&
                      (fresh.status == NavMeshPathStatus.PathComplete ||
                       fresh.status == NavMeshPathStatus.PathPartial);   // partial = walk as close as possible

            if (!ok)
            {
                Debug.Log($"[EntityOnMap] {name}: no path to {target} (status={fresh.status})");
                SetIdleState();
                yield break;
            }

            // Disable auto-replanning: prevents the agent changing direction
            // mid-move when another entity switches obstacle state
            navMeshAgent.autoRepath = false;
            navMeshAgent.path = fresh;

            // Two frames so remainingDistance is valid before the loop starts
            yield return null;
            yield return null;

            float elapsed = 0f;
            while (navMeshAgent.hasPath &&
                   navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            {
                if ((elapsed += Time.deltaTime) > 15f)
                {
                    Debug.LogWarning($"[EntityOnMap] {name}: movement timed out.");
                    break;
                }
                yield return null;
            }

            // ── Back to IDLE state ────────────────────────────────────────
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.autoRepath = true;
            navMeshAgent.ResetPath();
            navMeshAgent.enabled = false;

            yield return null; // let position settle before re-carving
            if (navMeshObstacle != null) navMeshObstacle.enabled = true;

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
                Debug.Log($"[EntityOnMap] Snapped {name} to NavMesh at {hit.position}");
            }
            else
                Debug.LogWarning($"[EntityOnMap] Could not snap {name} near {transform.position}");
        }
    }
}
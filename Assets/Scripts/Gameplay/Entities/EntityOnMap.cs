using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EntityOnMap : MonoBehaviour
{
    private Coroutine moveRoutine;
    private NavMeshAgent navMeshAgent;
    private NavMeshObstacle navMeshObstacle;
    private PathData cachedPath;

    [SerializeField] private float jumpDuration = 0.35f;
    [SerializeField] private float jumpHeight = 1.5f;

    public void Startup()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();

        if (navMeshAgent != null)
        {
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.enabled = true;
        }


        navMeshObstacle = GetComponent<NavMeshObstacle>();
        // Add NavMeshObstacle with carving enabled for dynamic avoidance
        if (navMeshObstacle == null)
        {
            navMeshObstacle = gameObject.AddComponent<NavMeshObstacle>();
            navMeshObstacle.enabled = false;
            navMeshObstacle.shape = NavMeshObstacleShape.Capsule;
            navMeshObstacle.center = Vector3.zero;
            navMeshObstacle.size = new Vector3(0.2f, 2f, 0f); // Adjust size as needed
            navMeshObstacle.carving = true;
        }
    }

    /// <summary>Helper to re-snap the agent to the NavMesh if it's off the surface.</summary>
    private void TrySnapToNavMesh()
    {
        if (navMeshAgent == null || navMeshAgent.isOnNavMesh) return;

        Debug.Log($"[EntityOnMap] TrySnapToNavMesh: Agent is off mesh. Attempting to snap...");
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            navMeshAgent.Warp(hit.position);
            Debug.Log($"[EntityOnMap] Successfully snapped to NavMesh at: {hit.position}");
        }
        else
        {
            Debug.LogWarning($"[EntityOnMap] Could not find NavMesh position near {transform.position}");
        }
    }

    public void TeleportTo(Vector3 position)
    {
        if (navMeshAgent == null) return;
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            navMeshAgent.Warp(hit.position);
    }

    /// <summary>Enable obstacle carving for pathfinding preview. Disables the NavMeshAgent.</summary>
    public void EnableObstacleCarving()
    {
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.enabled = false;
        }

        if (navMeshObstacle != null && !navMeshObstacle.enabled)
        {
            navMeshObstacle.enabled = true;
        }
    }

    /// <summary>Disable obstacle carving after pathfinding preview. Re-enables the NavMeshAgent.</summary>
    public void DisableObstacleCarving()
    {
        if (navMeshObstacle != null && navMeshObstacle.enabled)
        {
            navMeshObstacle.enabled = false;
        }
    }

    /// <summary>Re-enable the NavMeshAgent after obstacle carving is disabled and navmesh has rebuilt.</summary>
    public IEnumerator ReenableAgentAfterCarvingDisabled()
    {
        // Yield one frame for the NavMesh to rebuild after obstacle carving is disabled
        yield return null;

        if (navMeshAgent != null && !navMeshAgent.enabled)
        {
            navMeshAgent.enabled = true;
        }
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

    /// <summary>Start movement using a cached path. Use this for optimized pathfinding.</summary>
    public IEnumerator StartMoveRoutineWithPath(PathData pathData)
    {
        if (pathData == null || pathData.Path == null || pathData.Path.Count == 0)
        {
            yield break;
        }

        cachedPath = pathData;
        if (moveRoutine != null) StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(FollowPath(pathData.End));
        yield return moveRoutine;
        cachedPath = null;
    }

    private IEnumerator FollowPath(Vector3 target)
    {
        // The NavMesh carved by the obstacle is rebuilt asynchronously.
        // Yield one frame so the hole closes before the agent queries its position.
        yield return null;

        if (navMeshAgent == null)
        {
            yield break;
        }

        // Clear velocity before starting pathfinding to prevent jitter
        navMeshAgent.velocity = Vector3.zero;

        // Re-snap after the NavMesh update in case the agent missed the mesh.
        TrySnapToNavMesh();

        if (!navMeshAgent.isOnNavMesh)
        {
            yield break;
        }

        NavMeshPath path;
        bool pathCalculated;

        // Use cached path if available, otherwise calculate new one
        if (cachedPath != null && cachedPath.CachedNavMeshPath != null)
        {
            path = cachedPath.CachedNavMeshPath;
            pathCalculated = true;
        }
        else
        {
            path = new NavMeshPath();
            pathCalculated = navMeshAgent.CalculatePath(target, path);
        }

        if (!pathCalculated || path.status != NavMeshPathStatus.PathComplete)
        {
            yield break;
        }

        navMeshAgent.path = path;

        // Wait for the agent to reach the destination
        // The NavMeshAgent.remainingDistance will naturally decrease as the agent moves
        while (navMeshAgent.hasPath && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            yield return null;
        }

        // Ensure complete stop
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.ResetPath();
        moveRoutine = null;
    }

    private IEnumerator JumpTo(Vector3 target)
    {
        // Yield one frame for the NavMesh to rebuild after obstacle removal.
        yield return null;

        if (navMeshAgent == null) yield break;

        // Clear velocity before jump to prevent jitter
        navMeshAgent.velocity = Vector3.zero;

        TrySnapToNavMesh();

        if (!navMeshAgent.isOnNavMesh) yield break;

        Vector3 start = transform.position;
        NavMeshHit hit;
        if (!NavMesh.SamplePosition(target, out hit, 5f, NavMesh.AllAreas))
            hit.position = target;

        for (float t = 0; t < jumpDuration; t += Time.deltaTime)
        {
            float norm = Mathf.Clamp01(t / jumpDuration);
            Vector3 pos = Vector3.Lerp(start, hit.position, norm);
            pos.y += jumpHeight * 4f * norm * (1f - norm);
            navMeshAgent.Warp(pos);
            yield return null;
        }

        navMeshAgent.Warp(hit.position);
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.ResetPath();
        moveRoutine = null;
    }
}

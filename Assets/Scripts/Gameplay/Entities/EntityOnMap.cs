using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EntityOnMap : MonoBehaviour
{
    private Coroutine moveRoutine;
    private NavMeshAgent navMeshAgent;

    [SerializeField] private float jumpDuration = 0.35f;
    [SerializeField] private float jumpHeight = 1.5f;

    public void Startup()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        Debug.Log($"[EntityOnMap] Startup called. Found NavMeshAgent: {navMeshAgent != null}");

        if (navMeshAgent != null)
        {
            navMeshAgent.velocity = Vector3.zero;
            navMeshAgent.enabled = true;
            Debug.Log($"[EntityOnMap] NavMeshAgent initialized and enabled");
        }
        else
        {
            Debug.LogError($"[EntityOnMap] Failed to find NavMeshAgent on {gameObject.name}");
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

    private IEnumerator FollowPath(Vector3 target)
    {
        // The NavMesh carved by the obstacle is rebuilt asynchronously.
        // Yield one frame so the hole closes before the agent queries its position.
        yield return null;

        Debug.Log($"[EntityOnMap] FollowPath called. Target: {target}");

        if (navMeshAgent == null)
        {
            Debug.LogError($"[EntityOnMap] NavMeshAgent is null! Movement cannot proceed.");
            yield break;
        }

        Debug.Log($"[EntityOnMap] NavMeshAgent found. Current position: {navMeshAgent.transform.position}");

        // Clear velocity before starting pathfinding to prevent jitter
        navMeshAgent.velocity = Vector3.zero;

        // Re-snap after the NavMesh update in case the agent missed the mesh.
        TrySnapToNavMesh();

        if (!navMeshAgent.isOnNavMesh)
        {
            Debug.LogError($"[EntityOnMap] NavMeshAgent is not on NavMesh after TrySnapToNavMesh! Position: {navMeshAgent.transform.position}");
            yield break;
        }

        Debug.Log($"[EntityOnMap] NavMeshAgent is on NavMesh. Calculating path...");

        var path = new NavMeshPath();
        bool pathCalculated = navMeshAgent.CalculatePath(target, path);
        Debug.Log($"[EntityOnMap] Path calculation result: {pathCalculated}, Status: {path.status}");

        if (!pathCalculated || path.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogError($"[EntityOnMap] Path calculation failed or incomplete. PathCalculated: {pathCalculated}, Status: {path.status}");
            yield break;
        }

        Debug.Log($"[EntityOnMap] Valid path found with {path.corners.Length} corners");

        navMeshAgent.path = path;

        // Wait for the agent to reach the destination
        // The NavMeshAgent.remainingDistance will naturally decrease as the agent moves
        while (navMeshAgent.hasPath && navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            Debug.Log($"[EntityOnMap] Moving... Remaining: {navMeshAgent.remainingDistance}, Velocity: {navMeshAgent.velocity.magnitude}");
            yield return null;
        }

        // Ensure complete stop
        navMeshAgent.velocity = Vector3.zero;
        navMeshAgent.ResetPath();
        moveRoutine = null;
        Debug.Log($"[EntityOnMap] Movement completed. Final position: {navMeshAgent.transform.position}");
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Queue that executes actions in order, supporting optional delays.
/// Automatically starts processing when actions are enqueued.
/// </summary>
public class ActionQueue : MonoBehaviour
{
    private class QueuedAction
    {
        public Action Action;
        public float Delay;

        public QueuedAction(Action action, float delay = 0f)
        {
            Action = action;
            Delay = delay;
        }
    }

    private Queue<QueuedAction> queue = new();
    private bool isProcessing = false;

    /// <summary>
    /// True if there are no actions being processed or queued.
    /// </summary>
    public bool IsEmpty => queue.Count == 0 && !isProcessing;

    /// <summary>
    /// Enqueue a new action. Will automatically start processing if idle.
    /// </summary>
    public void Enqueue(Action action, float delay = 0f)
    {
        if (action == null) return;

        queue.Enqueue(new QueuedAction(action, delay));

        // Start processing immediately if not already
        if (!isProcessing)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    /// <summary>
    /// Processes the queue sequentially.
    /// </summary>
    private IEnumerator ProcessQueue()
    {
        isProcessing = true;

        while (queue.Count > 0)
        {
            var item = queue.Dequeue();

            // Execute the action
            item.Action?.Invoke();

            // Wait for the specified delay
            if (item.Delay > 0f)
                yield return new WaitForSeconds(item.Delay);
            else
                yield return null; // yield a frame to avoid locking
        }

        isProcessing = false;
    }

    /// <summary>
    /// Clears the queue immediately.
    /// </summary>
    public void ClearQueue()
    {
        queue.Clear();
    }
}

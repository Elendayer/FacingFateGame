using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace facingfate
{
    /// <summary>
    /// Queue that executes actions in order, supporting optional delays.
    /// Automatically starts processing when actions are enqueued.
    /// </summary>
    public class ActionQueue : MonoBehaviour
    {
        private class QueuedAction
        {
            public Action Action;
            public Func<IEnumerator> CoroutineAction;
            public bool IsCoroutine;
            public float Delay;
            public EntityScript Source;
            public bool IsPriority;

            public QueuedAction(Action action, float delay = 0f, EntityScript source = null, bool isPriority = false)
            {
                Action = action;
                Delay = delay;
                IsCoroutine = false;
                Source = source;
                IsPriority = isPriority;
            }

            public QueuedAction(Func<IEnumerator> coroutineAction, float delay = 0f, EntityScript source = null, bool isPriority = false)
            {
                CoroutineAction = coroutineAction;
                Delay = delay;
                IsCoroutine = true;
                Source = source;
                IsPriority = isPriority;
            }
        }

        private List<QueuedAction> queue = new();
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
            Enqueue(action, delay, null, false);
        }

        /// <summary>
        /// Enqueue a new action with an associated source entity. Will automatically start processing if idle.
        /// </summary>
        public void Enqueue(Action action, float delay, EntityScript source)
        {
            Enqueue(action, delay, source, false);
        }

        /// <summary>
        /// Enqueue a new action with priority (executed before non-priority actions). Will automatically start processing if idle.
        /// </summary>
        public void EnqueuePriority(Action action, float delay = 0f, EntityScript source = null)
        {
            Enqueue(action, delay, source, true);
        }

        private void Enqueue(Action action, float delay, EntityScript source, bool isPriority)
        {
            if (action == null) return;

            var newAction = new QueuedAction(action, delay, source, isPriority);

            if (isPriority)
            {
                // Insert at the front of priority actions (after the currently processing action if any)
                int insertIndex = queue.FindIndex(a => !a.IsPriority);
                if (insertIndex == -1)
                    queue.Add(newAction);
                else
                    queue.Insert(insertIndex, newAction);
            }
            else
            {
                queue.Add(newAction);
            }

            // Start processing immediately if not already
            if (!isProcessing)
            {
                StartCoroutine(ProcessQueue());
            }
        }

        /// <summary>
        /// Enqueue a coroutine-producing function. The queue will wait until the coroutine completes.
        /// </summary>
        public void Enqueue(Func<IEnumerator> coroutineAction, float delay = 0f)
        {
            Enqueue(coroutineAction, delay, null, false);
        }

        /// <summary>
        /// Enqueue a coroutine-producing function with an associated source entity. The queue will wait until the coroutine completes.
        /// </summary>
        public void Enqueue(Func<IEnumerator> coroutineAction, float delay, EntityScript source)
        {
            Enqueue(coroutineAction, delay, source, false);
        }

        /// <summary>
        /// Enqueue a coroutine-producing function with priority (executed before non-priority actions). The queue will wait until the coroutine completes.
        /// </summary>
        public void EnqueuePriority(Func<IEnumerator> coroutineAction, float delay = 0f, EntityScript source = null)
        {
            Enqueue(coroutineAction, delay, source, true);
        }

        private void Enqueue(Func<IEnumerator> coroutineAction, float delay, EntityScript source, bool isPriority)
        {
            if (coroutineAction == null) return;

            var newAction = new QueuedAction(coroutineAction, delay, source, isPriority);

            if (isPriority)
            {
                // Insert at the front of priority actions (after the currently processing action if any)
                int insertIndex = queue.FindIndex(a => !a.IsPriority);
                if (insertIndex == -1)
                    queue.Add(newAction);
                else
                    queue.Insert(insertIndex, newAction);
            }
            else
            {
                queue.Add(newAction);
            }

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
                var item = queue[0];
                queue.RemoveAt(0);

                if (item.IsCoroutine)
                {
                    // Run the coroutine and wait for it to finish
                    yield return StartCoroutine(item.CoroutineAction());
                }
                else
                {
                    // Execute the action
                    item.Action?.Invoke();
                }

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

        /// <summary>
        /// Removes all queued actions associated with a specific source entity.
        /// </summary>
        public void ClearActionsBySource(EntityScript source)
        {
            if (source == null) return;

            queue.RemoveAll(item => item.Source == source);
        }
    }
}
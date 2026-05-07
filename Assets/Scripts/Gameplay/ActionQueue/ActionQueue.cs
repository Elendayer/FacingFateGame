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
        /// Enqueue a CardActionSequence. The queue will wait until all actions complete.
        /// </summary>
        public void Enqueue(CardActionSequence cardSequence, float delay = 0f)
        {
            Enqueue(cardSequence, delay, null, false);
        }

        /// <summary>
        /// Enqueue a CardActionSequence with an associated source entity.
        /// </summary>
        public void Enqueue(CardActionSequence cardSequence, float delay, EntityScript source)
        {
            Enqueue(cardSequence, delay, source, false);
        }

        /// <summary>
        /// Enqueue a CardActionSequence with priority.
        /// </summary>
        public void EnqueuePriority(CardActionSequence cardSequence, float delay = 0f, EntityScript source = null)
        {
            Enqueue(cardSequence, delay, source, true);
        }

        private void Enqueue(CardActionSequence cardSequence, float delay, EntityScript source, bool isPriority)
        {
            if (cardSequence == null) return;

            // Wrap CardActionSequence in a coroutine for consistent handling
            Func<IEnumerator> sequenceCoroutine = () => cardSequence.Execute();
            var newAction = new QueuedAction(sequenceCoroutine, delay, source, isPriority);

            if (isPriority)
            {
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

                // Wait for the initial delay before executing
                if (item.Delay > 0f)
                    yield return new WaitForSeconds(item.Delay);

                if (item.IsCoroutine)
                {
                    IEnumerator cr = null;
                    try { cr = item.CoroutineAction(); }
                    catch (Exception e) { Debug.LogError($"[ActionQueue] Coroutine factory threw: {e}"); }
                    if (cr != null)
                        yield return StartCoroutine(SafeCoroutine(cr));
                }
                else
                {
                    try { item.Action?.Invoke(); }
                    catch (Exception e) { Debug.LogError($"[ActionQueue] Exception in queued action: {e}"); }
                    yield return null; // yield a frame to avoid locking
                }
            }

            isProcessing = false;
        }

        /// <summary>
        /// Wraps a coroutine so that exceptions inside it are caught and logged
        /// instead of propagating up and killing ProcessQueue permanently.
        /// </summary>
        private IEnumerator SafeCoroutine(IEnumerator inner)
        {
            while (true)
            {
                bool moveNext;
                object current;
                try
                {
                    moveNext = inner.MoveNext();
                    current  = inner.Current;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ActionQueue] Exception inside coroutine: {e}");
                    yield break;
                }
                if (!moveNext) yield break;
                yield return current;
            }
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
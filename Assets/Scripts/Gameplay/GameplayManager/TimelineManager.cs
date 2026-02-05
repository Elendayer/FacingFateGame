using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace facingfate
{
    public class TimelineManager : MonoBehaviour
    {
        public static TimelineManager Instance { get; private set; }

        public static Dictionary<string, List<ToSendTriggerReference>> Timeline = new();

        public static ActionQueue GlobalActionQueue;

        public static bool isPaused = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            // Ensure ActionQueue component exists
            if (GlobalActionQueue == null)
            {
                GlobalActionQueue = gameObject.GetComponent<ActionQueue>() ?? gameObject.AddComponent<ActionQueue>();
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;
        }


        #region Timeline
        public static void AddToTimeline(ToSendTriggerReference triggerRef)
        {
            if (triggerRef.OnTriggerReference == null) return;

            if (triggerRef.OnTriggerReference.Contains(GameplayRef.onTurnStart))
            {
                Timeline.TryAdd($"{TurnManager.Instance.CurrentRoundIndex}_{triggerRef.UserEntity.name}", new() { triggerRef });
            }
            else
            {
                Timeline.Last().Value.Add(triggerRef);
            }
        }

        public static List<ToSendTriggerReference> GetDataFromTimeline(
            EntityScript entity,
            TimelineFilter filter,
            object filterValue = null,
            int turnsAgo = int.MaxValue,
            bool isUser = true)
        {
            // Flatten Timeline into a list with turn index
            var flatTimeline = Timeline
                .Select(kvp => new { Key = kvp.Key, Triggers = kvp.Value })
                .SelectMany(t =>
                {
                    // Extract turn index from key
                    var split = t.Key.Split('_');
                    int turnIndex = int.Parse(split.First());
                    return t.Triggers.Select(tr => new { Trigger = tr, Turn = turnIndex });
                })
                .OrderByDescending(x => x.Turn) // Start from the most recent turn
                .ToList();

            var result = new List<ToSendTriggerReference>();

            foreach (var entry in flatTimeline)
            {
                if (TurnManager.Instance.CurrentRoundIndex - entry.Turn > turnsAgo)
                    break; // Stop if we exceeded the turn range

                var tr = entry.Trigger;

                bool matches = filter switch
                {
                    TimelineFilter.User => isUser
                        ? tr.UserEntity == entity
                        : tr.AffectedEntities != null && tr.AffectedEntities.Contains(entity),

                    TimelineFilter.CardData => tr.CardData != null && tr.CardData == filterValue as CardData,

                    TimelineFilter.CardIdentity => tr.CardData != null && tr.CardData.cardIdentities != null && tr.CardData.cardIdentities.Contains((CardIdentity)filterValue),

                    TimelineFilter.CardClass => tr.CardData != null && tr.CardData.cardClass == (CardClass)filterValue,

                    TimelineFilter.CardType => tr.CardData != null && tr.CardData.cardType == (CardType)filterValue,

                    _ => false
                };

                if (matches)
                    result.Add(tr);
            }

            return result;
        }
        public enum TimelineFilter
        {
            User,
            CardClass,
            CardIdentity,
            CardType,
            CardData
        }
        #endregion
    }
}
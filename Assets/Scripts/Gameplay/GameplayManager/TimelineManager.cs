using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using facingfate;

namespace facingfate
{
    public class TimelineManager : MonoBehaviour
    {
        public static TimelineManager Instance { get; private set; }

    public static Dictionary<string,List<ToSendTriggerReference>> Timeline = new();

    public static ActionQueue GlobalActionQueue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Instance = this;
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
        if(triggerRef.OnTriggerReference == null) return;

        if ( triggerRef.OnTriggerReference.Contains(GameplayRef.onTurnStart))
        {
            Timeline.TryAdd($"{TurnManager.Instance.CurrentRoundIndex}_{triggerRef.UserEntity.name}", new() { triggerRef });
        }
        public static List<TriggerRef> GetDataFromTimeline(
            EntityScript entity,
            TimelineFilter filter,
            object filterValue = null,
            int turnsAgo = int.MaxValue,
            bool isUser = true)
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
                if (TurnManager.Instance.CurrentRoundIndex - entry.Turn > turnsAgo)
                    break; // Stop if we exceeded the turn range

        var result = new List<ToSendTriggerReference>();

                bool matches = filter switch
                {
                    TimelineFilter.User => isUser ? tr.UserEntity == entity : tr.AffectedEntities.Contains(entity),
                    TimelineFilter.CardData => tr.CardData == filterValue as CardData,
                    TimelineFilter.CardIdentity => tr.CardData.cardIdentities.Contains((CardIdentity)filterValue),
                    TimelineFilter.CardClass => tr.CardData.cardClass == (CardClass)filterValue,
                    TimelineFilter.CardType => tr.CardData.cardType == (CardType)filterValue,
                    _ => false
                };

                if (matches)
                    result.Add(tr);
            }

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
        public enum TimelineFilter
        {
            User,
            CardClass,
            CardIdentity,
            CardType,
            CardData
        }
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
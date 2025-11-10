using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    public static List<TriggerRef> Timeline = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public void StartUp()
    {

    }

    public static void AddToTimeline(TriggerRef triggerRef)
    {
        Timeline.Add(triggerRef);
    }

    public static List<TriggerRef> GetDataFromTimeline(int entityId, CardData cardData, int count = 1, bool isUser = true)
    {
        List<TriggerRef> triggerRefs;
        if (isUser)
        {
            triggerRefs = Timeline.Where(tr => tr.UserId == entityId && tr.CardData == cardData).Take(count).ToList();
            return triggerRefs;
        }
        else
        {
            triggerRefs = Timeline.Where(tr => tr.AffectedEntityId == entityId && tr.CardData == cardData).Take(count).ToList();
            return triggerRefs;
        }
    }
    public static List<TriggerRef> GetDataFromTimeline(int entityId, CardIdentity cardIdentity, int count = 1, bool isUser = true)
    {
        List<TriggerRef> triggerRefs;
        if (isUser)
        {
            triggerRefs = Timeline.Where(tr => tr.UserId == entityId && tr.CardData.cardIdentities.Contains(cardIdentity)).Take(count).ToList();
            return triggerRefs;
        }
        else
        {
            triggerRefs = Timeline.Where(tr => tr.AffectedEntityId == entityId && tr.CardData.cardIdentities.Contains(cardIdentity)).Take(count).ToList();
            return triggerRefs;
        }
    }

    public static List<TriggerRef> GetDataFromTimeline(int entityId, GameplayRef gameplayRef, int count = 1, bool isUser = true)
    {
        List<TriggerRef> triggerRefs;
        if (isUser)
        {
            triggerRefs = Timeline.Where(tr => tr.UserId == entityId && tr.OnTriggerReference.Contains(gameplayRef)).Take(count).ToList();
            return triggerRefs;
        }
        else
        {
            triggerRefs = Timeline.Where(tr => tr.AffectedEntityId == entityId && tr.OnTriggerReference.Contains(gameplayRef)).Take(count).ToList();
            return triggerRefs;
        }
    }
    public static List<TriggerRef> GetDataFromTimeline(int entityId, int count = 1, bool isUser = true)
    {
        List<TriggerRef> triggerRefs;
        if (isUser)
        {
            triggerRefs = Timeline.Where(tr => tr.UserId == entityId).Take(count).ToList();
            return triggerRefs;
        }
        else
        {
            triggerRefs = Timeline.Where(tr => tr.AffectedEntityId == entityId  && tr.OnTriggerReference != null).Take(count).ToList();
            return triggerRefs;
        }
    }
}
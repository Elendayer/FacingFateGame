using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using static TimelineManager;

/// <summary>
/// Utility class to enqueue actions into the global action queue.
/// Handles card effects, repeat logic, and delays.
/// </summary>
public static class ActionQueueUtility
{
    /// <summary>
    /// Enqueue a card action into the global action queue.
    /// Supports repeats and delays between repeats.
    /// </summary>
    /// <param name="source">The entity executing the card.</param>
    /// <param name="cardData">Card data containing effects and repeat info.</param>
    /// <param name="targetingData">Data about targeted entities and tiles.</param>
    public static void EnqueueCardExecution(
           EntityScript source,
           CardData cardData,
           TargetingModeData targetingData,
           GameObject cardObj = null,
           float repeatDelay = 0.25f)
    {
        // 1️ Pre-combat trigger
        EnqueueAction(() =>
        {
            CombatUtility.HandlePreCombatTrigger(targetingData.targetedEntities, cardData);
        });

        // 2️ Card effect repeats
        int repeats = Mathf.Max(cardData.repeats_u, 1);
        for (int i = 0; i < repeats; i++)
        {
            float delay = repeatDelay * i;

            GlobalActionQueue.Enqueue(() =>
            {
                ApplyCardEffects(source, cardData, targetingData);
            }, delay);
        }

        // 3️ Discard card after effects (only for player)
        if (cardObj != null && source is PlayerScript)
        {
            EnqueueAction(() =>
            {
                HandManager.Instance.DiscardCard(cardObj);
            });
        }

        // 4️ Update stats for owner and targets
        EnqueueAction(() =>
        {
            // Owner stats
            cardData.Owner.entityStats.UpdateStats();

            // Targeted entities stats
            foreach (EntityScript e in targetingData.targetedEntities)
            {
                e.entityStats.UpdateStats();
            }
        });
    }

    /// <summary>
    /// Applies the card's effects to targeted entities and tiles.
    /// </summary>
    private static void ApplyCardEffects(
		EntityScript source,
		CardData cardData,
		TargetingModeData targetingData)
	{

		Debug.Log($"Applying Card Effects for Card: {cardData.cardName} from Source: {source.name}");
        // Apply effects to targeted entities
        foreach (EntityScript target in targetingData.targetedEntities)
		{
			cardData.CardEffect?.Invoke(source, target, cardData);
		}

		// Apply effects to targeted tiles
		foreach (Vector3Int tile in targetingData.targetedTiles)
		{
			cardData.CardEffectGround?.Invoke(source, tile, cardData);
		}
	}

    /// <summary>
    /// Enqueue a movement action through the global action queue.
    /// Will execute the movement and call onComplete when finished.
    /// </summary>
    public static void EnqueueMovement(
        EntityOnMap entityOnMap,
        PathData pathData,
        Action onComplete = null)
    {

		Debug.Log($"Enqueuing Movement Action for Entity at {entityOnMap.currentCell} to {pathData.End}");
        GlobalActionQueue.Enqueue(() =>
        {
            // Start the movement as a coroutine
            entityOnMap.StartCoroutine(MoveCoroutine(entityOnMap, pathData, onComplete));
        });
    }

    /// <summary>
    /// Coroutine that performs the movement and invokes callback on completion.
    /// </summary>
    private static IEnumerator MoveCoroutine(
        EntityOnMap entityOnMap,
        PathData pathData,
        Action onComplete)
    {
        yield return entityOnMap.StartMove(pathData);
        onComplete?.Invoke();
    }

    /// <summary>
    /// Enqueue a generic action into the global action queue.
    /// </summary>
    public static void EnqueueAction(Action action, float delay = 0f)
    {
        if (action == null) return;

		Debug.Log($"Enqueuing Action in Global Queue");
        GlobalActionQueue.Enqueue(action, delay);
    }
}

using UnityEngine;
using UnityEngine.UI;
using facingfate;


public class HandUtility : MonoBehaviour
{
    /// <summary>
    /// Internal helper to perform the actual discard operation on a card.
    /// Moves the card to discard parent, resets state, and adds to discard stack.
    /// </summary>
    private static void DiscardCardInternal(CardScript cardScript)
    {
        if (cardScript == null) return;

        GameObject cardGO = cardScript.gameObject;

        // Reset transform
        RectTransform rectTransform = cardGO.transform as RectTransform;
        if (rectTransform != null)
        {
            TransformUtility.ZeroLocalRectTransform(rectTransform);
            rectTransform.localScale = Vector3.one;
        }

        // Move to discard pile
        cardGO.transform.SetParent(DeckManager.Instance.discardParent, false);

        // Reset card state
        cardScript.SetHidden();
        cardScript.ResetCard();

        // Add to discard stack if not already there
        if (!DeckManager.Instance.discardStack.Contains(cardGO))
            DeckManager.Instance.discardStack.Push(cardGO);
    }

    /// <summary>
    /// Discard a card using its CardScript component.
    /// </summary>
    public static void Discard(CardScript cs)
    {
        DiscardCardInternal(cs);
    }

    /// <summary>
    /// Discard a card using its GameObject.
    /// </summary>
    public static void Discard(GameObject gameObject)
    {
        if (gameObject == null) return;
        CardScript cs = gameObject.GetComponent<CardScript>();
        DiscardCardInternal(cs);
    }

    /// <summary>
    /// Set to 1 before a player card's effects execute so that Draw allows one
    /// extra slot (the played card will be discarded after effects finish).
    /// Reset to 0 just before the card is discarded in step 3.
    /// </summary>
    public static int SlotsBeingFreedByPlayedCard = 0;

    /// <summary>
    /// Draw a card from the deck and add it to the hand.
    /// Handles deck exhaustion by reshuffling discard pile if needed.
    /// </summary>
    public static bool Draw(GameplayRef eventRef)
    {
        DeckManager deckManager = DeckManager.Instance;
        HandManager handManager = HandManager.Instance;

        if (deckManager == null || handManager == null)
            return false;

        // Check if deck is empty and attempt to reshuffle from discard (only if discard has cards)
        if (deckManager.cardStack.Count == 0)
        {
            if (deckManager.discardStack.Count == 0)
            {
                Debug.Log("[HandUtility] Deck and discard pile are both empty. Cannot draw card.");
                return false;
            }

            Debug.Log("[HandUtility] Deck is empty. Reshuffling discard pile into deck.");
            deckManager.Player_ShuffleDiscard();

            // Verify reshuffle was successful
            if (deckManager.cardStack.Count == 0)
            {
                Debug.LogWarning("[HandUtility] Reshuffle failed - deck is still empty after shuffling discard.");
                return false;
            }
        }

        // Check if hand is full (subtract slots being freed by a card currently being played)
        if (handManager.cardsInHand.Count - SlotsBeingFreedByPlayedCard >= handManager.maxHandsize)
        {
            Debug.Log("[HandUtility] Hand is full. Cannot draw card.");
            return false;
        }

        // Draw the top card
        GameObject topCard = deckManager.cardStack.Pop();
        handManager.AddCard(topCard);

        // Reveal the card
        CardScript cs = topCard.GetComponent<CardScript>();
        if (cs != null)
            cs.SetRevealed();

        // Update visuals and trigger event
        RefreshDeckVisuals();
        GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { eventRef }, null, null));

        return true;
    }

    /// <summary>
    /// Refreshes the deck pile visuals.
    /// </summary>
    private static void RefreshDeckVisuals()
    {
        DeckManager deckManager = DeckManager.Instance;
        if (deckManager == null) return;

        if (deckManager.deckParent != null)
        {
            deckManager.deckParent.GetComponent<DiscardPileVisualizer>()?.Refresh();
            LayoutRebuilder.ForceRebuildLayoutImmediate(deckManager.deckParent);
        }
    }
}
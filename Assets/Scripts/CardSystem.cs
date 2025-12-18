using System.Collections.Generic;
using UnityEngine;

public static class CardSystem
{
    /// <summary>
    /// Führt eine Karte aus: prüft Stamina, zieht Kosten ab, führt Repeats aus, discardet und refresht die Hand-Locks.
    /// Targeting/TurnOrder bewusst minimal – target kann null sein (z. B. Self-Karten).
    /// </summary>
    public static bool TryPlay(CardScript cardUI, EntityScript user, EntityScript targetOrNull, out string reason)
    {
        reason = null;
        if (cardUI == null || cardUI.cardData == null || user == null)
        {
            reason = "Invalid card or user";
            return false;
        }

        var data = cardUI.cardData;
        int cost = data.Cost;
        if (user.entityStats.CurrentStamina < cost)
        {
            reason = "Not enough Stamina";
            return false;
        }

        // Kosten bezahlen
       // user.entityStats.CurrentStamina.AddModifier(new ) -= cost;

        // Direkte Ausführung (ohne Stack), inkl. Repeats
        int repeats = Mathf.Max(1, data.Repeats);
        for (int i = 0; i < repeats; i++)
        {
              //  data.ActivateCardEffect(new List<EntityScript> { targetOrNull}, cardUI.gameObject);
        }

        // Discard
        DeckManager.Instance.DiscardCardFromHand(cardUI.gameObject);

        // Hand sperren/freigeben basierend auf aktueller Stamina
        HandUI.RefreshHandLocks(user);

        // Optionaler Hook (du hast bereits Reference-Events)
        //GameEvents.TriggerReferenceEvent(gameplayReference.cardPlayedRef);

        return true;
    }
}

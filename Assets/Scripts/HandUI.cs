using UnityEngine;

namespace facingfate
{
    public static class HandUI
    {
        /// <summary>
        /// Refreshes lock + alpha for every card in hand based on current stamina.
        /// Safe to call during tutorial — only ADDS locks for unaffordable cards,
        /// never removes tutorial-applied locks.
        /// </summary>
        public static void RefreshHandLocks(EntityScript user)
        {
            if (HandManager.Instance == null || user == null) return;

            float stamina = user.entityStats.CurrentStamina;
            bool tutorialActive = TutorialCombatManager.Instance != null && TutorialCombatManager.Instance.IsActive;

            foreach (var go in HandManager.Instance.cardsInHand)
                ApplyStaminaLockToCard(go, stamina, tutorialActive);
        }

        /// <summary>
        /// Applies stamina-based lock + alpha to a single card.
        /// Called from HandManager.AddCard so newly drawn cards are also covered.
        /// </summary>
        public static void ApplyStaminaLockToCard(GameObject cardGO, EntityScript user)
        {
            if (cardGO == null || user == null) return;
            bool tutorialActive = TutorialCombatManager.Instance != null && TutorialCombatManager.Instance.IsActive;
            ApplyStaminaLockToCard(cardGO, user.entityStats.CurrentStamina, tutorialActive);
        }

        private static void ApplyStaminaLockToCard(GameObject cardGO, float stamina, bool tutorialActive)
        {
            if (cardGO == null) return;
            var cs = cardGO.GetComponent<CardScript>();
            if (cs?.cardData == null) return;
            var cg = cardGO.GetComponent<CanvasGroup>();

            bool staminaLocked = stamina < cs.cardData.Cost;

            if (tutorialActive)
            {
                // During tutorial: only add lock/dim for unaffordable cards.
                // Never remove a lock the tutorial already set — tutorial manages unlock.
                if (staminaLocked)
                {
                    cs.SetupLock(true);
                    if (cg != null) cg.alpha = 0.4f;
                }
            }
            else
            {
                cs.SetupLock(staminaLocked);
                if (cg != null) cg.alpha = staminaLocked ? 0.4f : 1f;
            }
        }
    }
}

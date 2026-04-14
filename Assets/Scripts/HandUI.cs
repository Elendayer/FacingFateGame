using UnityEngine;

namespace facingfate
{
    public static class HandUI
    {
        public static void RefreshHandLocks(EntityScript user)
            {
                if (HandManager.Instance == null || user == null) return;
                float stamina = user.entityStats.CurrentStamina;

            foreach (var go in HandManager.Instance.cardsInHand)
            {
                var cs = go.GetComponent<CardScript>();
                if (cs?.cardData == null) continue;

                bool lockIt = stamina < cs.cardData.Cost;
                cs.SetupLock(lockIt);
            }
        }
    }
}

using UnityEngine;

namespace facingfate
{
    public class PlayerScript : EntityScript
    {
        private void Awake()
        {
            base.entityAffiliation = EntityAffiliation.Player;
        }
        public override void StartUp()
        {
            base.StartUp();

            Debug.Log($"[PlayerScript] Setup complete for {name}");
            DeckManager.Instance.BuildDeckFromIDs(this);
        }

        public override void StartTurn()
        {
            base.StartTurn();

            if (entityStats.IsStunned)
            {
                ActionQueueUtility.EnqueueAction(() =>
                {
                    GameEvents.TriggerTurnEnd();
                    entityStats.IsStunned = false;
                });

                return;
            }

            // Refresh hand locks after stamina reset so existing cards dim/undim correctly.
            HandUI.RefreshHandLocks(this);
        }


        public override void DrawCards(int toDraw)
        {
            for (int i = 0; i < toDraw; i++)
            {
                DeckManager.Instance.Player_DrawTopCard();
            }
        }
        public override void DiscardCards(int toDiscard)
        {
            for (int i = 0; i < toDiscard; i++)
            {
                DeckManager.Instance.Player_DiscardRandomCardFromHand();
            }
        }
    }
}
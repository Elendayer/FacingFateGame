using UnityEngine;
using TMPro;

namespace facingfate
{
    public class TurnCounterUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text roundText;
        [SerializeField] private TMP_Text currentTurnText;

        private void OnEnable()
        {
            GameEvents.OnTurnStart += Refresh;
            GameEvents.OnTurnEnd += Refresh;
            GameEvents.OnCombatStart += Refresh;
        }

        private void OnDisable()
        {
            GameEvents.OnTurnStart -= Refresh;
            GameEvents.OnTurnEnd -= Refresh;
            GameEvents.OnCombatStart -= Refresh;
        }

        private void Refresh()
        {
            TurnManager tm = TurnManager.Instance;
            if (tm == null) return;

            if (roundText != null)
                roundText.text = $"Round {tm.CurrentRoundIndex}";

            if (currentTurnText != null)
            {
                EntityScript current = tm.TurnOrder != null && tm.TurnOrder.Count > 0
                    ? tm.TurnOrder[tm.CurrentTurnIndex]
                    : null;

                if (current != null)
                {
                    // Spieler oder NPC Name anzeigen
                    string name = current.GetComponent<PlayerScript>() != null
                        ? current.gameObject.name
                        : current.GetComponent<NonPlayerScript>()?.npcData?.name ?? current.gameObject.name;

                    currentTurnText.text = $"{name}'s Turn";
                }
                else
                {
                    currentTurnText.text = "-";
                }
            }
        }
    }
}

using UnityEngine;
using TMPro;

namespace facingfate
{
    public class CardListEntryUI : MonoBehaviour
    {
        //[SerializeField] private float previewScale = 0.7f; // 70% der Originalgröße

        public void SetCard(CardScript sourceCard)
        {
            if (sourceCard == null) return;

            CardScript ownCS = GetComponent<CardScript>();
            if (ownCS != null)
            {
                ownCS.cardData = sourceCard.cardData;
                ownCS.ApplyCardDataVisuals();
                if (ownCS.cardBack != null)
                    ownCS.cardBack.SetActive(false);
            }
        }
        public void SetText(string text) { }

        public void SetTooltip(string body) { }

    }
}

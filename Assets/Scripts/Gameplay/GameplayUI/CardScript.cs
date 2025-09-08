using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardScript : MonoBehaviour
{
    [SerializeField]

    public CardData cardData;
    public Image artworkRenderer;
    public GameObject cardBack;
    public TextMeshProUGUI nameText; // Optional: if you're using UI elements
    public TextMeshProUGUI descriptionText;

    public bool isLocked;
    public bool inPlay;

    public void SetupLock(bool b)
    {
        isLocked = b;

        if (isLocked)
        {
            GetComponent<DraggableCard>().enabled = false;
        }
        else
        {
            GetComponent<DraggableCard>().enabled = true;
        }
    }

    public void SetCard(CardData data)
    {
        cardData = data;
        ApplyCardDataVisuals();
    }
    public void ApplyCardDataVisuals()
    {
        if (cardData != null)
        {
            cardData.SetCardDescription(cardData.Owner, cardData);
            artworkRenderer.sprite = cardData.cardArtwork;
            nameText.text = cardData.cardName;
            descriptionText.text = cardData.cardDescription;
            Debug.Log($"[Card] {cardData.cardName} -> Owner={cardData.Owner?.name} (inst {cardData.GetHashCode()})");
        }
    }

    public void ResetCard()
    {
        GetComponent<DraggableCard>().enabled = false;
    }

    internal void SetHidden()
    {
        cardBack.SetActive (true); 
        GetComponent<DraggableCard>().enabled = false ;
    }

    internal void SetRevealed()
    {
        cardBack.SetActive(false);
        GetComponent<DraggableCard>().enabled = true;
    }
}
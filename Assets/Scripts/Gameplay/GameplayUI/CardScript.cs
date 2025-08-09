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
            GetComponent<DraggableCardUI>().enabled = false;
        }
        else
        {
            GetComponent<DraggableCardUI>().enabled = true;
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
            cardData.SetCardDescription(PlayerManager.Instance, cardData);
            artworkRenderer.sprite = cardData.cardArtwork;
            nameText.text = cardData.cardName;
            descriptionText.text = cardData.cardDescription;
        }
    }

    public void ResetCard()
    {
        GetComponent<DraggableCardUI>().enabled = false;
    }

    internal void SetHidden()
    {
        cardBack.SetActive (true); 
        GetComponent<DraggableCardUI>().enabled = false ;
    }

    internal void SetRevealed()
    {
        cardBack.SetActive(false);
        GetComponent<DraggableCardUI>().enabled = true;
    }
}
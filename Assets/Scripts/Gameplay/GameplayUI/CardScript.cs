using TMPro;
using UnityEngine;

public class CardScript : MonoBehaviour
{
    [SerializeField]

    public CardData cardData;
    public UnityEngine.UI.Image artworkRenderer;
    public GameObject cardBack;
    public TextMeshProUGUI nameText; // Optional: if you're using UI elements
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI range;
    public TextMeshProUGUI cost;

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

    public void ApplyCardDataVisuals()
    {
        if (cardData != null)
        {
            cardData.CardDescription(cardData.Owner, cardData);
            artworkRenderer.sprite = cardData.cardArtwork;
            nameText.text = cardData.cardName;
            cost.text = $"{cardData.Cost}";
            descriptionText.text = cardData.cardDescription;

            switch (cardData.targetingData.cardSelectionType)
            {
                case CardTargetSelection.Single:
                    switch (cardData.targetingData.CardTargetAffiliation)
                    {
                        case CardTargetAffiliation.Ally:
                            range.text = $"Single Ally in {cardData.Range} Tiles";
                            break;
                        case CardTargetAffiliation.Enemy:
                            range.text = $"Single Enemy in {cardData.Range} Tiles";
                            break;
                        case CardTargetAffiliation.Self:
                            range.text = "Self Target";
                            break;
                        default:
                            range.text = $"Single Target in {cardData.Range} Tiles";
                            break;
                    }
                    break;

                case CardTargetSelection.Ring:
                    {
                        switch (cardData.targetingData.CardTargetAffiliation)
                        {
                            case CardTargetAffiliation.Ally:
                                range.text = $"Ring {cardData.Area}, Ally Targets in {cardData.Range} Tiles";
                                break;
                            case CardTargetAffiliation.Enemy:
                                range.text = $"Ring {cardData.Area}, Enemy Targets in {cardData.Range} Tiles";
                                break;
                            default:
                                range.text = $"Ring {cardData.Area}, in {cardData.Range} Tiles";
                                break;
                        }
                    }
                        break;
                    
                    case CardTargetSelection.RingSelf:
                    {
                        switch (cardData.targetingData.CardTargetAffiliation)
                        {
                            case CardTargetAffiliation.Ally:
                                range.text = $"Ring {cardData.Area} from Self, Ally Targets";
                                break;
                            case CardTargetAffiliation.Enemy:
                                range.text = $"Ring {cardData.Area} from Self, Enemy Targets";
                                break;
                            default:
                                range.text = $"Ring {cardData.Area} from Self";
                                break;
                        }
                    }
                    break;
                case CardTargetSelection.Radius:
                    {
                        switch (cardData.targetingData.CardTargetAffiliation)
                        {
                            case CardTargetAffiliation.Ally:
                                range.text = $"Radius {cardData.Area}, Ally Targets in {cardData.Range} Tiles";
                                break;
                            case CardTargetAffiliation.Enemy:
                                range.text = $"Radius {cardData.Area}, Enemy Targets in {cardData.Range} Tiles";
                                break;
                            default:
                                range.text = $"Radius {cardData.Area}, in {cardData.Range} Tiles";
                                break;
                        }
                    }
                    break;
                case CardTargetSelection.LineFree:
                    {
                        switch (cardData.targetingData.CardTargetAffiliation)
                        {
                            case CardTargetAffiliation.Ally:
                                range.text = $"Line Free, Ally Targets, up to {cardData.Range} Tiles";
                                break;
                            case CardTargetAffiliation.Enemy:
                                range.text = $"Line Free, Enemy Targets, up to {cardData.Range} Tiles";
                                break;
                            default:
                                range.text = $"Line Free, up to {cardData.Range} Tiles";
                                break;
                        }
                    }
                    break;
                case CardTargetSelection.LineSelf:
                    {
                        switch (cardData.targetingData.CardTargetAffiliation)
                        {
                            case CardTargetAffiliation.Ally:
                                range.text = $"Line from Self, Ally Targets, up to {cardData.Range} Tiles";
                                break;
                            case CardTargetAffiliation.Enemy:
                                range.text = $"Line from Self, Enemy Targets, up to {cardData.Range} Tiles";
                                break;
                            default:
                                range.text = $"Line from Self, up to {cardData.Range} Tiles";
                                break;
                        }
                    }
                    break;
                case CardTargetSelection.Cone:
                    {
                        switch (cardData.targetingData.CardTargetAffiliation)
                        {
                            case CardTargetAffiliation.Ally:
                                range.text = $"Cone from Self, Ally Targets, {cardData.Range} by {cardData.Area}";
                                break;
                                case CardTargetAffiliation.Enemy:
                                range.text = $"Cone from Self, Enemy Targets, {cardData.Range} by {cardData.Area}";
                                break;
                                default:
                                range.text = $"Cone from Self, {cardData.Range} by {cardData.Area}";
                                break;

                        }
                    }
                    break;
                case CardTargetSelection.Select:
                    switch (cardData.targetingData.CardTargetAffiliation)
                    {
                        case CardTargetAffiliation.Ally:
                            range.text = $"Select, Ally Targets, {cardData.Area} targets within {cardData.Range} Tiles";
                            break;
                        case CardTargetAffiliation.Enemy:
                            range.text = $"Select, Enemy Targets, {cardData.Area} targets within {cardData.Range} Tiles";
                            break;
                        default:
                            range.text = $"Select, {cardData.Area} targets within {cardData.Range} Tiles";
                            break;

                    }
                    break;
                case CardTargetSelection.All:
                    {
                        switch (cardData.targetingData.CardTargetAffiliation)
                        {
                            case CardTargetAffiliation.Ally:
                                range.text = $"All Ally Targets";
                                break;
                            case CardTargetAffiliation.Enemy:
                                range.text = $"All Enemy Targets";
                                break;
                            default:
                                range.text = $"All Targets";
                                break;
                        }
                    }
                    break;
            }
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
        ApplyCardDataVisuals();
    }
}
using System.Collections.Generic;
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

            range.text = GetRangeText(cardData);
        }
    }
    public static string GetRangeText(CardData cardData)
    {
        var t = cardData.targetingData;

        List<string> parts = new();

        switch (t.cardSelectionType)
        {
            case CardTargetingModeType.Single:
                parts.Add("Single Target");
                break;

            case CardTargetingModeType.Ring:
                parts.Add($"Ring, {cardData.Radius} by {cardData.Area}");
                break;

            case CardTargetingModeType.Radius:
                parts.Add($"Radius, {cardData.Radius}");
                break;

            case CardTargetingModeType.LineFree:
                parts.Add($"Line Free, with maximum length {cardData.Area}");
                break;

            case CardTargetingModeType.LineSelf:
                parts.Add($"Line from Self, {cardData.Range}");
                break;

            case CardTargetingModeType.Cone:
                parts.Add($"Cone from Self, {cardData.Range} by {cardData.Area}");
                break;

            case CardTargetingModeType.Select:
                parts.Add($"Select, {cardData.MaxTarget} targets");
                break;

            case CardTargetingModeType.All:
                parts.Add("All");
                break;
        }

        //
        // 2. Affiliation if applicable
        //
        if (t.CardTargetAffiliation != CardTargetAffiliation.Self)
        {
            if (t.CardTargetAffiliation == CardTargetAffiliation.Ally)
                parts.Add("Ally Targets");
            else if (t.CardTargetAffiliation == CardTargetAffiliation.Enemy)
                parts.Add("Enemy Targets");
            else
                parts.Add("");
        }
        else
        {
            // Special case: Self-target overrides everything else.
            if (t.cardSelectionType == CardTargetingModeType.Single)
                return "Self Target";

            parts.Add("Self Target");
        }

        //
        // 3. Range (only if mode uses range)
        //
        if (t.cardSelectionType is not CardTargetingModeType.All)
        {
            if (t.cardSelectionType == CardTargetingModeType.LineFree ||
                t.cardSelectionType == CardTargetingModeType.LineSelf)
                parts.Add($"up to {cardData.Range} Tiles");
            else if (t.cardSelectionType != CardTargetingModeType.Cone) // cone already included its own dimensions
                parts.Add($"in {cardData.Range} Tiles");
        }

        //
        // 4. Final join
        //
        return string.Join(", ", parts);
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
using System.Collections;
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

        StartCoroutine("DescriptionUpdate");
    }

    private IEnumerator DescriptionUpdate()
    {
        while (true)
        {
            descriptionText.text = FormatCardDescription(cardData);
            range.text = FormatCardRange(cardData);

            yield return new WaitForSeconds(0.2f);
        }
    }
    private string FormatCardDescription(CardData d)
    {
        return d.cardDescription
                .Replace("Power", d.Power.ToString())
                .Replace("Damage", d.Damage.ToString())
                .Replace("Healing", d.Duration.ToString())
                .Replace("Duration", d.Duration.ToString())
                .Replace("Repeats", d.Repeats.ToString())
                .Replace("Range", d.Range.ToString())
                .Replace("Area", d.Area.ToString())
                .Replace("Radius", d.Radius.ToString())
                .Replace("MaxTarget", d.MaxTarget.ToString());
    }
    private string FormatCardRange(CardData d)
    {
        return GetRangeText(cardData);
    }

    public static string GetRangeText(CardData cardData)
    {
        var t = cardData.targetingData;

        List<string> parts = new();

        switch (t.cardTargetingMode)
        {
            case CardTargetingMode.Single:
                parts.Add("Single Target");
                break;

            case CardTargetingMode.Ring:
                parts.Add($"Ring, {cardData.Radius} by {cardData.Area}");
                break;

            case CardTargetingMode.Radius:
                parts.Add($"Radius, {cardData.Radius}");
                break;

            case CardTargetingMode.LineFree:
                parts.Add($"Line Free, with maximum length {cardData.Area}");
                break;

            case CardTargetingMode.LineSelf:
                parts.Add($"Line from Self, {cardData.Range}");
                break;

            case CardTargetingMode.Cone:
                parts.Add($"Cone from Self, {cardData.Range} by {cardData.Area}");
                break;

            case CardTargetingMode.Select:
                parts.Add($"Select, {cardData.MaxTarget} targets");
                break;

            case CardTargetingMode.All:
                parts.Add("All");
                break;
        }

        // if effect uses Vision
        if (t.EffectUsesVision)
        {
            parts.Add("Blocked by Obstacles,");
        }
        else
        {
            parts.Add(",");
        }

        //  Affiliation if applicable
        if (t.CardTargetAffiliation != CardTargetAffiliation.Self)
        {
            if (t.CardTargetAffiliation == CardTargetAffiliation.Ally)
            {
                parts.Add("Targeting Allies");
            }
            else if (t.CardTargetAffiliation == CardTargetAffiliation.Enemy)
            {
                parts.Add("Targeting Enemies");
            }
            else
            {
                parts.Add("Targeting Everything");
            }

            // if targeting uses Vision
            if (t.TargetingUsesVision)
            {
                parts.Add("in Sight,");
            }
        }
        else
        {
            parts.Add("Targets Self");
        }

        // Range (only if mode uses range)
        if (t.cardTargetingMode is not CardTargetingMode.All)
        {
            parts.Add($"within {cardData.Range} Tiles");
        }

        // Final join
        return string.Join(" ", parts);
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
using System;
using UnityEngine;
using UnityEngine.UI;

public class CardSlot : DraggableTarget
{
    [Header("Gameplay")]
    public CardScript cardScript;
    private Image Panel;
    public bool isLocked;

    [Header("References")]
    public int slotIndex;

    private void Awake()
    {
        Panel = GetComponent<Image>();
        SetupLock(isLocked);
    }
    public void SetupLock(bool b)
    {
        isLocked = b;

        if (isLocked)
        {
            Panel.color = Color.blue;
        }
        else
        {
            Panel.color = Color.green;
        }
    }
    public void AttachCardToSlot(GameObject cardObject)
    {
        cardScript = cardObject.GetComponent<CardScript>();
        cardScript.SetupLock(isLocked);

        UtilityScript.ZeroLocalRectTransform(cardObject.GetComponent<RectTransform>());

        cardObject.transform.SetParent(transform, false);
    }

    public void PlayCardOntoSlot(CardScript cs)
    {
        cs.cardData.targetCard = cardScript;
        cs.cardData.CardEffect(PlayerManager.Instance, EnemyManager.Instance, cs.cardData);
        cardScript.ApplyCardDataVisuals();

        UtilityScript.Discard(cs);
    }
    public void DiscardCardFromSlot()
    {
        GameObject cs = cardScript.gameObject;
        if (cardScript.gameObject == null) return;
        UtilityScript.Discard(cs);
    }
    public void ResetSlot()
    {
        cardScript = null;

        if (isLocked == false)
        {
            Panel.color = Color.green;
        }
        else
        {
            Panel.color = Color.blue;
        }
    }
}
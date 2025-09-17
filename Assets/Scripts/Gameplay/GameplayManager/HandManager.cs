using System.Collections.Generic;
using UnityEngine;

public class HandManager : MonoBehaviour
{
    public static HandManager Instance { get; private set; }

    public Transform handAnchor;
    public GameObject cardPrefab;
    public float cardSpacing = 100f;
    public float cardFanAngle = -5f;

    public float maxHandsize = 8;

    public List<GameObject> cardsInHand = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void AddCard(GameObject newCard)
    {
        newCard.GetComponent<CardScript>().SetHidden();
        newCard.transform.SetParent(handAnchor, false);
        cardsInHand.Add(newCard);
    }
    public void RemoveCard(GameObject cardObject)
    {
        if (cardsInHand.Contains(cardObject))
        {
            cardsInHand.Remove(cardObject);
        }
    }
    public void DiscardCard(GameObject cardObject)
    {
        if (cardsInHand.Contains(cardObject))
        {
            HandUtility.Discard(cardObject);
        }
    }
    public void DiscardAllInHand()
    {
        while (cardsInHand.Count < 0)
        {
            HandUtility.Discard(cardsInHand[0]);
        }
    }
}

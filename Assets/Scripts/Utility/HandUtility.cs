using UnityEngine;
using facingfate;


public class HandUtility : MonoBehaviour
{

        public static void Discard(CardScript cs)
        {
            cs.gameObject.transform.SetParent(DeckManager.Instance.discardParent);
            cs.SetHidden();
            cs.ResetCard();
            if (!DeckManager.Instance.discardStack.Contains(cs.gameObject))
                DeckManager.Instance.discardStack.Push(cs.gameObject);
        }

        public static void Discard(GameObject gameObject)
        {
            CardScript cs = gameObject.GetComponent<CardScript>();

            cs.gameObject.transform.SetParent(DeckManager.Instance.discardParent);

            cs.SetHidden();  // Hide discarded card
            cs.ResetCard();
        }
    }

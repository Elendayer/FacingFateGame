using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

namespace facingfate
{
    public class DebugListScript : MonoBehaviour
    {
        private List<string> list = new List<string>();

        private void Start()
        {
            AddListener();
        }

        private void AddListener()
        {
            GameEvents.OnGameplayReference += AddToList;
        }

        public void AddToList(ToSendTriggerReference trigger)
        {
            if (trigger.CardData != null)
            {
                if (!list.Contains(trigger.CardData.cardName))
                {
                    list.Add(trigger.CardData.cardName);
                    GameObject a = Instantiate(new GameObject(), transform);
                    TextMeshProUGUI b = a.AddComponent<TextMeshProUGUI>();
                    b.text = trigger.CardData.cardName;
                }
            }
        }
    }
}

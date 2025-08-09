using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : EntityManager
{   // Singleton instance
    public static EnemyManager Instance { get; private set; }

    public EnemyAI enemyAI;

    protected void Awake()
    {
        // Singleton enforcement
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Ensure only one instance exists
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Optional: persist between scenes
    }
    private void Start()
    {
        enemyAI.Setup();
    }

    [System.Serializable]
    public class EnemyAI
    {
        public List<int> EnemyCardsByID;

        public List<CardData> cards;

        public void Setup()
        {
            foreach ( int id in EnemyCardsByID)
            {
                cards.Add (CardDatabase.GetCardById (id, EnemyManager.Instance));
            }

            Act();
        }
        public void Act()
        {
            AddIntention();
        }
        private void AddIntention()
        {
            CardData nextCard;
            int nextPriority = 0;

            foreach (CardData card in cards)
            {
                if (card.cooldown <= 0)
                {
                    if(EnemyManager.Instance.CurrentHealth.HasReference(card.triggerCondition))
                    {
                        nextCard = card;
                    }

                    if (card.priority < nextPriority)
                    {
                        nextPriority = card.priority;
                        nextCard = card;
                    }
                    card.cooldown--;
                }
            }
        }
    } 
}

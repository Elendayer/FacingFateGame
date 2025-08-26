using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : EntityScript
{
    public EnemyAI enemyAI;

    private void Start()
    {
        enemyAI.Setup(this);
    }

    [System.Serializable]
    public class EnemyAI
    {
        public List<int> EnemyCardsByID;

        public List<CardData> EnemyDeck;

        public void Setup(EnemyScript enemyScript)
        {
            foreach (int id in EnemyCardsByID)
            {
                EnemyDeck.Add(CardDatabase.GetCardById(id, enemyScript));
            }

            Act();
        }
        public void Act()
        {
        }
    }
}
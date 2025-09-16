using System;
using System.Collections.Generic;
using UnityEngine;

public class NonPlayerScript : EntityScript
{
    private void Awake()
    {
        base.entityAffiliation = EntityAffiliation.Enemy;
    }

    [Header("AI")]
    public NpcAI enemyAI;

    [System.Serializable]
    public class NpcAI
    {

    }
}
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : EntityScript
{
    private void Awake()
    {
        base.entityAffiliation = EntityAffiliation.Player;
    }
}
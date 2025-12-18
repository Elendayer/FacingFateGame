using System;
using System.Collections.Generic;

[Serializable]
public class NpcAiBias
{
    public string id;

    public Dictionary<GameplayRef,int> cardReferenceBias = new Dictionary<GameplayRef,int>();
    public Dictionary<GameplayRef,int> targetReferenceBias = new Dictionary<GameplayRef,int>();
    public Dictionary<CardIdentity,int> identityBias = new Dictionary<CardIdentity,int>();

    public RepositionCondition RepositionCondition;

    public NpcAiBias Clone()
    {
        return new NpcAiBias
        {
            id = this.id,
            cardReferenceBias = new Dictionary<GameplayRef, int>(this.cardReferenceBias),
            targetReferenceBias = new Dictionary<GameplayRef, int>(this.targetReferenceBias),
            identityBias = new Dictionary<CardIdentity, int>(this.identityBias),

            RepositionCondition = this.RepositionCondition
        };
    }

}

public enum RepositionCondition
{
    surrounded,
    lowHealth,
    preferRanged,
}

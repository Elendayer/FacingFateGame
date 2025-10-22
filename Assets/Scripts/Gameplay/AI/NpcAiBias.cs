using System;
using System.Collections.Generic;

[Serializable]
public class NpcAiBias
{
    public string id;

    public Dictionary<Intention,int> intentionBias = new Dictionary<Intention, int>();
    public Dictionary<gameplayRef,int> refBias = new Dictionary<gameplayRef,int>();
    public Dictionary<CardIdentity,int> identityBias = new Dictionary<CardIdentity,int>();

    public RepositionCondition RepositionCondition;

    public int BiasCalc(CardData cardData)
    {
        if (cardData == null) return 0;

        if (cardData.CardAiBias == null) return 0;

        int biasValue = 0;

        if (intentionBias.TryGetValue(cardData.CardAiBias.Intention, out int iValue))
        {
            biasValue += iValue;
        }
        foreach(gameplayRef gr in cardData.GameplayReferences)
        {
            if (refBias.TryGetValue(gr, out int rValue))
            {
                biasValue += rValue;
            }
        }
        foreach(CardIdentity identity in cardData.cardIdentities)
        {
            if(identityBias.TryGetValue(identity, out int idenityValue))
            {
                biasValue += idenityValue;
            }
        }
        return biasValue;
    }

    public NpcAiBias Clone()
    {
        return new NpcAiBias
        {
            id = this.id,
            intentionBias = new Dictionary<Intention, int>(this.intentionBias),
            refBias = new Dictionary<gameplayRef, int>(this.refBias),
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

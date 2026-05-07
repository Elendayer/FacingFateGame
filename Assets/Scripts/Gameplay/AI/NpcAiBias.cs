using System;
using System.Collections.Generic;

namespace facingfate
{
    [Serializable]
    public class NpcAiBias
    {
        public string id;

        public Dictionary<GameplayRef, float> cardReferenceBias = new Dictionary<GameplayRef, float>();
        public Dictionary<GameplayRef, float> targetReferenceBias = new Dictionary<GameplayRef, float>();
        public Dictionary<CardIdentity, float> identityBias = new Dictionary<CardIdentity, float>();

        public RepositionCondition RepositionCondition;

        public NpcAiBias Clone()
        {
            return new NpcAiBias
            {
                id = this.id,
                cardReferenceBias = new Dictionary<GameplayRef, float>(this.cardReferenceBias),
                targetReferenceBias = new Dictionary<GameplayRef, float>(this.targetReferenceBias),
                identityBias = new Dictionary<CardIdentity, float>(this.identityBias),

                RepositionCondition = this.RepositionCondition
            };
        }

    }

    public enum RepositionCondition
    {
        surrounded,
        lowHealth,
        preferRanged,
        always,
    }
}

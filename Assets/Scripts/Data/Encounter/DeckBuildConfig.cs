using System;
using System.Collections.Generic;

namespace facingfate
{
    /// <summary>
    /// Inline config for random deck generation. Assign directly on RandomEncounterManager in the Inspector.
    /// </summary>
    [Serializable]
    public class DeckBuildConfig
    {
        [Serializable]
        public class DeckTypeRatio
        {
            public CardType cardType = CardType.Technique;
            public int count = 3;
        }

        public List<DeckTypeRatio> ratios = new()
        {
            new DeckTypeRatio { cardType = CardType.Technique, count = 5 },
            new DeckTypeRatio { cardType = CardType.Ability,   count = 3 },
            new DeckTypeRatio { cardType = CardType.Skill,     count = 2 },
        };
    }
}

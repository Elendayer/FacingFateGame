using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace facingfate
{
    /// <summary>
    /// Builds a random list of card IDs for a given class and deck config.
    /// Cards are drawn from the registered CardDatabase.
    /// Falls back to Neutral cards of the same type if the class pool is too small.
    /// Allows duplicates (draws with replacement).
    /// </summary>
    public static class RandomDeckBuilder
    {
        /// <summary>
        /// Build a random deck.
        /// </summary>
        /// <param name="cardClass">The class to draw cards from (e.g. Spearman, Assassin).</param>
        /// <param name="ratios">How many cards of each CardType to include.</param>
        /// <returns>List of card IDs ready to assign to entity.deckCardIDs.</returns>
        public static List<string> Build(CardClass cardClass, List<DeckBuildConfig.DeckTypeRatio> ratios)
        {
            var result = new List<string>();
            var allCards = CardDatabase.GetAllCards();

            foreach (var ratio in ratios)
            {
                if (ratio.count <= 0) continue;

                // Primary pool: cards matching the requested class + type
                var pool = allCards
                    .Where(c => c.cardClass == cardClass && c.cardType == ratio.cardType)
                    .Select(c => c.cardID)
                    .ToList();

                // Fallback: add Neutral cards of same type if pool is insufficient
                if (pool.Count == 0)
                {
                    pool = allCards
                        .Where(c => c.cardClass == CardClass.Neutral && c.cardType == ratio.cardType)
                        .Select(c => c.cardID)
                        .ToList();

                    if (pool.Count == 0)
                    {
                        Debug.LogWarning($"[RandomDeckBuilder] No cards found for class {cardClass}, type {ratio.cardType} — skipping {ratio.count} slots.");
                        continue;
                    }
                }

                // Draw with replacement (duplicates allowed)
                for (int i = 0; i < ratio.count; i++)
                {
                    result.Add(pool[Random.Range(0, pool.Count)]);
                }
            }

            return result;
        }

        /// <summary>
        /// Convenience overload using a DeckBuildConfig.
        /// </summary>
        public static List<string> Build(CardClass cardClass, DeckBuildConfig config)
        {
            if (config == null)
            {
                Debug.LogError("[RandomDeckBuilder] DeckBuildConfig is null.");
                return new List<string>();
            }
            return Build(cardClass, config.ratios);
        }
    }
}

using System.Collections.Generic;

namespace facingfate
{
    public class PlayerData
    {
        public int baseStrength     = 10;
        public int baseDexterity    = 10;
        public int baseWisdom       = 10;
        public int baseIntelligence = 10;
        public int baseEndurance    = 10;
        public int baseTenacity     = 10;
    }

    public static class PlayerDatabase
    {
        private static readonly Dictionary<CardClass, PlayerData> Lookup = new();

        public static void Register(CardClass cardClass, PlayerData data)
        {
            Lookup[cardClass] = data;
        }

        /// <summary>
        /// Returns PlayerData for the given class, or null if not registered.
        /// </summary>
        public static PlayerData Get(CardClass cardClass)
        {
            return Lookup.TryGetValue(cardClass, out PlayerData data) ? data : null;
        }

        public static void RegisterAll()
        {
            // Str=Strength, Dex=Dexterity, Wis=Wisdom, Int=Intelligence, End=Endurance, Ten=Tenacity
            // HP = Tenacity * 50 | Stamina = Endurance * 5 | Draw/turn = floor(Wisdom / 2)

            // Spearman — Sum 71
            // Frontliner: massive HP (1000), hits hard (Str 16), draws few but big (4 cards)
            Register(CardClass.Spearman, new PlayerData
            {
                baseStrength     = 16,
                baseDexterity    = 8,
                baseWisdom       = 8,
                baseIntelligence = 7,
                baseEndurance    = 12,
                baseTenacity     = 20,
            });

            // Assassin — Sum 75
            // Glass cannon: low HP (500), high Dex, chains combos via 6 draws
            Register(CardClass.Assassin, new PlayerData
            {
                baseStrength     = 12,
                baseDexterity    = 18,
                baseWisdom       = 13,
                baseIntelligence = 10,
                baseEndurance    = 12,
                baseTenacity     = 10,
            });

            // Mystic — Sum 71
            // Pure magic DPS: fragile (500 HP), highest Int (20), most stamina (70), draws 7
            Register(CardClass.Mystic, new PlayerData
            {
                baseStrength     = 5,
                baseDexterity    = 8,
                baseWisdom       = 14,
                baseIntelligence = 20,
                baseEndurance    = 14,
                baseTenacity     = 10,
            });

            // Physician — Sum 74
            // Support survivor: sturdy (700 HP), draws most (8), high Int for healing output
            Register(CardClass.Physician, new PlayerData
            {
                baseStrength     = 7,
                baseDexterity    = 10,
                baseWisdom       = 16,
                baseIntelligence = 15,
                baseEndurance    = 12,
                baseTenacity     = 14,
            });
        }
    }
}

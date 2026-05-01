using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace facingfate
{
    public class NpcDatabase : MonoBehaviour
    {
        private static Dictionary<string, NpcData> NpcLookup = new Dictionary<string, NpcData>();

        public static void RegisterNpc(NpcData npc)
        {
            if (!NpcLookup.ContainsKey(npc.id))
            {
                NpcLookup[npc.id] = npc;
                //Debug.Log($"Registered card: {card.cardName} (ID: {card.cardID})");
            }
            else
            {
                Debug.LogWarning($"Duplicate card ID detected: {npc.id}");
            }
        }
        public static NpcData GetNpcById(string id, EntityScript entity)
        {
            if (!NpcLookup.TryGetValue(id, out var blueprint) || blueprint == null)
                return null;

            NpcData npc = blueprint.Clone(entity);

            return npc;
        }

        public static List<NpcData> GetAllNpcs()
        {
            return new List<NpcData>(NpcLookup.Values);
        }

        public static void RegisterAll()
        {
            RegisterNpcs();
        }

        private static void RegisterNpcs()
        {
            // ── Tutorial ──────────────────────────────────────────────────────────
            // Weak melee dummy. Strike-heavy so the player learns basic attack flow.
            // Sting appears ~every 2-3 turns — enough to trigger a visible Poison DoT
            // without overwhelming a new player.
            // Stats: low Strength (non-threatening hits) + low Tenacity (dies fast).
            // Bias: Balanced — no identity weights, plays highest-scored card → predictable.
            RegisterNpc(new NpcData()
            {
                id = "Npc_Tutorial_Dummy",
                name = "Training Dummy",
                aiBias = AiBiasDatabase.GetBiasById("Balanced"),

                cardIds = new List<string>()
                {
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Sting",
                    "Neutral_Tech_Sting",
                    "Neutral_Tech_Sting",
                    "Neutral_Tech_Sting",
                },

                baseStrength     = 8,
                baseDexterity    = 10,
                baseTenacity     = 1,
                baseEndurance    = 10,
                baseIntelligence = 6,
                baseWisdom       = 6,
            });

            RegisterNpc(new NpcData()
            {
                id = "Npc_Wolf_Normal",
                name = "Wolf",
                aiBias = AiBiasDatabase.GetBiasById("Aggressive_Melee"),

                cardIds = new List<string>()
                {
                    "Neutral_Tech_Bite",
                    "Neutral_Tech_Bite",
                    "Neutral_Tech_Bite",
                    "Neutral_Tech_Bite",
                    "Neutral_Tech_Bite",
                    "Neutral_Tech_Claw",
                    "Neutral_Tech_Claw",
                    "Neutral_Tech_Claw",
                    "Neutral_Tech_Claw",
                    "Neutral_Tech_Claw",
                    "Neutral_Abil_Howl",
                    "Neutral_Abil_Howl",
                    "Neutral_Abil_Growl",
                    "Neutral_Abil_Growl",
                    "Neutral_Tech_Gnaw",
                    "Neutral_Tech_Gnaw",
                    "Neutral_Tech_Jump",
                    "Neutral_Tech_Jump",
                },

                baseStrength = 16,
                baseDexterity = 12,
                baseTenacity = 13,
                baseEndurance = 14,
                baseIntelligence = 4,
                baseWisdom = 5,

            });

            RegisterNpc(new NpcData()
            {
                id = "Npc_Wolf_Summon",
                name = "Summoned Wolf",
                aiBias = AiBiasDatabase.GetBiasById("Aggressive_Melee"),

                cardIds = new List<string>()
                {
                    "Neutral_Tech_Bite",
                    "Neutral_Tech_Bite",
                    "Neutral_Tech_Bite",
                    "Neutral_Tech_Bite",
                    "Neutral_Tech_Bite",
                    "Neutral_Tech_Claw",
                    "Neutral_Tech_Claw",
                    "Neutral_Tech_Claw",
                    "Neutral_Tech_Claw",
                    "Neutral_Tech_Claw",
                },

                baseTenacity = 8,
                baseIntelligence = 4,
                baseWisdom = 4,
            });

            RegisterNpc(new NpcData()
            {
                id = "Npc_Bandit_Archer",
                name = "Bandit Archer",
                aiBias = AiBiasDatabase.GetBiasById("Aggressive_Ranged"),

                cardIds = new List<string>()
                {
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Multi_Shot",
                    "Assassin_Tech_Multi_Shot",
                    "Assassin_Tech_Multi_Shot",
                    "Assassin_Tech_Bouncing_Shot",
                    "Neutral_Abil_Focus",
                    "Neutral_Abil_Focus",
                    "Assassin_Abil_Eye_of_the_Nighthawk",
                    "Assassin_Abil_Eye_of_the_Nighthawk",
                    "Neutral_Spell_Summon_Wolf",
                    "Neutral_Spell_Summon_Wolf",
                },

                baseDexterity = 20,
                baseEndurance = 12,
                baseTenacity = 8,

            });

            RegisterNpc(new NpcData()
            {
                id = "Npc_Bandit_Lord",
                name = "Bandit Lord",
                aiBias = AiBiasDatabase.GetBiasById("Balanced"),

                cardIds = new List<string>()
                {
                    "Neutral_Tech_Charge",
                    "Neutral_Tech_Charge",
                    "Neutral_Tech_Heavy_Blow",
                    "Neutral_Tech_Heavy_Blow",
                    "Neutral_Tech_Heavy_Blow",
                    "Neutral_Tech_Heavy_Blow",
                    "Neutral_Tech_Punch",
                    "Neutral_Tech_Punch",
                    "Neutral_Tech_Punch",
                    "Neutral_Tech_Punch",
                    "Neutral_Abil_Guard_Up",
                    "Neutral_Abil_Guard_Up",
                    "Neutral_Abil_Guard_Up",
                    "Spearman_Tech_EarthShattering_Pole",
                    "Spearman_Tech_EarthShattering_Pole",
                    "Spearman_Tech_EarthShattering_Pole",
                    "Neutral_Abil_Howl",
                    "Neutral_Abil_Growl",
                    "Spearman_Tech_Sky_Piercing_Leap",
                    "Spearman_Tech_Iron_wall_reversal",
                    "Spearman_Tech_Whirling_ward",
                    "Spearman_Tech_Sweep",
                    "Spearman_Tech_Sweep",
                    "Spearman_Tech_Sweep",
                    "Neutral_Abil_Summon_Wolf",
                    "Neutral_Tech_Sunder",
                    "Neutral_Abil_Warcry"
                },

                baseStrength = 20,
                baseTenacity = 25,
                baseEndurance = 16,
            });

            RegisterNpc(new NpcData()
            {
                id = "Mystic_Test_Npc",
                name = "Mystic",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),

                cardIds = new List<string>()
            {
                "Mystic_Tech_Mind_Shock",
                "Mystic_Tech_Staff_Swing",
                "Mystic_Tech_Absorb_Qi",
                "Mystic_Abil_Dancing_Shadow",
                "Mystic_Abil_Meditation",
                "Mystic_Spell_Illusionary_Double",
                "Mystic_Spell_Phantom_Spear_Battalion",
                "Mystic_Spell_Warp_Intention",
                "Mystic_Spell_Sleepwalking",
                "Mystic_Spell_Spectral_Barrier",
                "Mystic_Spell_Spacial_Reversal",
                "Mystic_Spell_Bloody_Hex",
                "Mystic_Spell_Venom_Hex",
                "Mystic_Spell_Crimson_Hex",
                "Mystic_Spell_Mental_Chains",
                "Mystic_Spell_Rainbow_Hex",
                "Mystic_Spell_Pure_Flames",
                //"Mystic_Curse_Psychic_Backlash",
                //"Mystic_Bless_Inner_Calm",
            },

                baseWisdom = 16,
            });

            RegisterNpc(new NpcData()
            {
                id = "Assassin_Test_Npc",
                name = "Assassin",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),
                cardIds = new List<string>()
            {
                "Ass_Tech_Shadowfang_Strike",
                "Ass_Tech_Dance_of_a_Hundred_Cuts",
                "Ass_Tech_Lotus_Death_Kiss",
                "Ass_Tech_Moonlit_Needlestorm",
                "Ass_Tech_Black_Lotus_Needle",
                "Ass_Tech_Moon_Piercing_Arrow",
                "Ass_Tech_Midnight_Rain",
                "Ass_Tech_Bouncing_Shot",
                "Ass_Tech_Barbed_Needle_Volley",
                "Ass_Tech_Merciful_Headshot",
                "Ass_Tech_Crimson_Thorn_Array",
                "Ass_Abil_Phantom_Step",
                "Ass_Abil_Apply_Scorching_Blood_Venom",
                "Ass_Abil_Apply_Black_Lotus_Venom",
                "Ass_Abil_Apply_Dazzlying_Numbing_Venom",
                "Ass_Abil_Reapply_Venom",
                "Ass_Abil_Eye_of_the_Nighthawk",
                //"Ass_Curse_Fumble",
                //"Ass_Bless_Lucky_Strike",
            }
            });

            RegisterNpc(new NpcData()
            {
                id = "Physician_Test_Npc",
                name = "Physician",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),
                cardIds = new List<string>()
            {
                "Phy_Tech_Jade_Needle_Acupuncture",
                "Phy_Tech_Bloodletting",
                "Phy_Tech_Formation_of_the_Hundred_Remedies",
                "Phy_Tech_Venomous_Grip",
                "Phy_Tech_Needle_of_the_Flowing_River",
                "Phy_Abil_Gather",
                "Phy_Abil_Toxic_Remedy_Paradox",
                "Phy_Abil_Poison_Barbs",
                "Phy_Abil_Doctors_Footwork",
                "Phy_Spell_Jade_Needle_Resonance",
                "Phy_Spell_Breath_of_the_Jade_Lotus",
                "Phy_Item_Brew_of_a_Hundred_Herbs",
                "Phy_Item_Elixir_of_a_Hundred_Herbs",
                "Phy_Item_Pill_of_a_Hundred_Herbs",
                "Phy_Item_Crimson_Rejuvenation_Brew",
                "Phy_Item_Crimson_Rejuvenation_Elixir",
                "Phy_Item_Crimson_Rejuvenation_Pill",
                "Phy_Item_Brew_of_Unbroken_Will",
                "Phy_Item_Elixir_of_Unbroken_Will",
                "Phy_Item_Pill_of_Unbroken_Will",
                "Phy_Item_Soaring_Dragon_Brew",
                "Phy_Item_Soaring_Dragon_Elixir",
                "Phy_Item_Soaring_Dragon_Pill",
                "Phy_Item_Crystal_Cleansing_Balm",
                "Phy_Item_Mandrake_Poison_Cloud",
                //"Phy_Curse_Alchemists_Misstep",
                //"Phy_Item_Mythical_Herb",
            }
            });

            // ── Random Encounter Pool ─────────────────────────────────────────────
            // Inspector power values: Brawler=3, Scout=3, Spear=4, Alchemist=4, Rogue=5, Hexer=5, Oni=6

            // Cheap melee grunt. Pure neutral cards, no class techs.
            // High Str, slow Dex — hits hard but telegraphed.
            RegisterNpc(new NpcData()
            {
                id = "Npc_Street_Brawler",
                name = "Street Brawler",
                aiBias = AiBiasDatabase.GetBiasById("Balanced"),

                cardIds = new List<string>()
                {
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Heavy_Blow",
                    "Neutral_Tech_Heavy_Blow",
                    "Neutral_Tech_Charge",
                    "Neutral_Tech_Charge",
                    "Neutral_Tech_Quick_Jab",
                    "Neutral_Tech_Quick_Jab",
                    "Neutral_Tech_Shove",
                    "Neutral_Tech_Shove",
                    "Neutral_Abil_Recover",
                    "Neutral_Abil_Warcry",
                },

                baseStrength     = 16,
                baseDexterity    = 9,
                baseTenacity     = 11,
                baseEndurance    = 13,
                baseIntelligence = 7,
                baseWisdom       = 7,
            });

            // Light archer — arrows + multishot + focus. No wolf summon.
            // Fragile, fast, punishes players who don't close the gap.
            RegisterNpc(new NpcData()
            {
                id = "Npc_Forest_Scout",
                name = "Forest Scout",
                aiBias = AiBiasDatabase.GetBiasById("Balanced"),

                cardIds = new List<string>()
                {
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Arrow_Shot",
                    "Assassin_Tech_Multi_Shot",
                    "Assassin_Tech_Multi_Shot",
                    "Assassin_Tech_Bouncing_Shot",
                    "Neutral_Abil_Focus",
                    "Neutral_Abil_Focus",
                    "Assassin_Abil_Eye_of_the_Nighthawk",
                    "Neutral_Tech_Step_Back",
                    "Neutral_Tech_Step_Back",
                },

                baseStrength     = 8,
                baseDexterity    = 17,
                baseTenacity     = 8,
                baseEndurance    = 10,
                baseIntelligence = 9,
                baseWisdom       = 10,
            });

            // Spear archetype — sweeps + earthshatter + defensive abils.
            // Balanced stats, reliable damage + some mitigation.
            RegisterNpc(new NpcData()
            {
                id = "Npc_Spear_Fighter",
                name = "Spear Fighter",
                aiBias = AiBiasDatabase.GetBiasById("Balanced"),

                cardIds = new List<string>()
                {
                    "Neutral_Tech_Quick_Jab",
                    "Neutral_Tech_Quick_Jab",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Abil_Recover",
                    "Neutral_Abil_Recover",
                    "Spear_Tech_Dragon_Tail_Sweep",
                    "Spear_Tech_Dragon_Tail_Sweep",
                    "Spear_Tech_Salamander_Tail_Sweep",
                    "Spear_Tech_Earthshatter_Pole",
                    "Spear_Tech_Earthshatter_Pole",
                    "Spear_Abil_Unyielding_Spear_Stance",
                    "Spear_Abil_Iron_Wall_Reversal",
                    "Spear_Abil_Whirling_Ward",
                },

                baseStrength     = 14,
                baseDexterity    = 12,
                baseTenacity     = 12,
                baseEndurance    = 12,
                baseIntelligence = 8,
                baseWisdom       = 9,
            });

            // Physician archetype — poison items + needle + light self-heal.
            // Low Str, high Int/Wis, relies on DoT and items.
            RegisterNpc(new NpcData()
            {
                id = "Npc_Physician_Alchemist",
                name = "Poison Alchemist",
                aiBias = AiBiasDatabase.GetBiasById("Balanced"),

                cardIds = new List<string>()
                {
                    "Neutral_Tech_Quick_Jab",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Step_Back",
                    "Neutral_Tech_Step_Back",
                    "Neutral_Abil_Recover",
                    "Physician_Item_Brew_of_a_Hundred_Herbs",
                    "Physician_Item_Brew_of_a_Hundred_Herbs",
                    "Physician_Item_Crimson_Rejuvenation_Brew",
                    "Physician_Item_Crimson_Rejuvenation_Brew",
                    "Physician_Item_Mandrake_Poison_Cloud",
                    "Physician_Abil_Poison_Barbs",
                    "Physician_Abil_Poison_Barbs",
                    "Physician_Tech_Needle_of_the_Flowing_River",
                    "Physician_Tech_Needle_of_the_Flowing_River",
                    "Physician_Abil_Doctors_Footwork",
                },

                baseStrength     = 8,
                baseDexterity    = 10,
                baseTenacity     = 10,
                baseEndurance    = 12,
                baseIntelligence = 14,
                baseWisdom       = 12,
            });

            // Assassin archetype — phantom step, shadowfang, venom setup.
            // High Dex, mobile, applies venom then burst.
            RegisterNpc(new NpcData()
            {
                id = "Npc_Assassin_Rogue",
                name = "Shadow Rogue",
                aiBias = AiBiasDatabase.GetBiasById("Balanced"),

                cardIds = new List<string>()
                {
                    "Neutral_Tech_Quick_Jab",
                    "Neutral_Tech_Quick_Jab",
                    "Neutral_Tech_Quick_Jab",
                    "Neutral_Tech_Double_Cut",
                    "Neutral_Tech_Double_Cut",
                    "Neutral_Abil_Recover",
                    "Assassin_Abil_Phantom_Step",
                    "Assassin_Abil_Phantom_Step",
                    "Assassin_Tech_Shadowfang_Strike",
                    "Assassin_Tech_Shadowfang_Strike",
                    "Assassin_Tech_Dance_of_a_Hundred_Cuts",
                    "Assassin_Tech_Dance_of_a_Hundred_Cuts",
                    "Assassin_Abil_Apply_Scorching_Blood_Venom",
                    "Assassin_Abil_Apply_Black_Lotus_Venom",
                    "Assassin_Tech_Bouncing_Shot",
                },

                baseStrength     = 10,
                baseDexterity    = 18,
                baseTenacity     = 9,
                baseEndurance    = 10,
                baseIntelligence = 11,
                baseWisdom       = 9,
            });

            // Mystic archetype — hexes + mind shock + spectral barrier.
            // Very low Str, max Wis, fragile but disrupts with debuffs.
            RegisterNpc(new NpcData()
            {
                id = "Npc_Mystic_Hexer",
                name = "Hex Caster",
                aiBias = AiBiasDatabase.GetBiasById("Balanced"),

                cardIds = new List<string>()
                {
                    "Neutral_Tech_Quick_Jab",
                    "Neutral_Tech_Quick_Jab",
                    "Neutral_Tech_Step_Back",
                    "Neutral_Abil_Recover",
                    "Neutral_Abil_Recover",
                    "Mystic_Spell_Spectral_Barrier",
                    "Mystic_Spell_Spectral_Barrier",
                    "Mystic_Tech_Mind_Shock",
                    "Mystic_Tech_Mind_Shock",
                    "Mystic_Tech_Mind_Shock",
                    "Mystic_Spell_Bloody_Hex",
                    "Mystic_Spell_Venom_Hex",
                    "Mystic_Spell_Crimson_Hex",
                    "Mystic_Spell_Sleepwalking",
                    "Mystic_Spell_Pure_Flames",
                },

                baseStrength     = 7,
                baseDexterity    = 9,
                baseTenacity     = 9,
                baseEndurance    = 10,
                baseIntelligence = 11,
                baseWisdom       = 16,
            });

            // Oni archetype — heavy brute, charge + gnaw + earthshatter.
            // Max Str/Ten, very low Dex/Int — slow but tanky with hard-hitting cards.
            RegisterNpc(new NpcData()
            {
                id = "Npc_Oni_Brute",
                name = "Oni Brute",
                aiBias = AiBiasDatabase.GetBiasById("Balanced"),

                cardIds = new List<string>()
                {
                    "Neutral_Tech_Charge",
                    "Neutral_Tech_Charge",
                    "Neutral_Tech_Charge",
                    "Neutral_Tech_Heavy_Blow",
                    "Neutral_Tech_Heavy_Blow",
                    "Neutral_Tech_Heavy_Blow",
                    "Neutral_Tech_Heavy_Blow",
                    "Neutral_Tech_Gnaw",
                    "Neutral_Tech_Gnaw",
                    "Neutral_Abil_Growl",
                    "Neutral_Abil_Howl",
                    "Spear_Tech_Earthshatter_Pole",
                    "Spear_Tech_Earthshatter_Pole",
                    "Spear_Tech_Sky_Reaching_Leap",
                    "Neutral_Tech_Sunder",
                    "Neutral_Abil_Warcry",
                },

                baseStrength     = 17,
                baseDexterity    = 8,
                baseTenacity     = 15,
                baseEndurance    = 16,
                baseIntelligence = 6,
                baseWisdom       = 6,
            });
        }
    }

    public class NpcData
    {
        public string id;
        public string name;
        public NpcAiBias aiBias = new();

        public List<string> cardIds = new();

        //base stats
        public int baseStrength = 10;
        public int baseDexterity = 10;
        public int baseWisdom = 10;
        public int baseIntelligence = 10;
        public int baseEndurance = 10;
        public int baseTenacity = 10;

        public NpcData Clone(EntityScript entity)
        {
            return new NpcData
            {
                id = this.id,
                name = this.name,
                aiBias = this.aiBias,
                cardIds = this.cardIds,

                baseStrength = this.baseStrength,
                baseDexterity = this.baseDexterity,
                baseWisdom = this.baseWisdom,
                baseIntelligence = this.baseIntelligence,
                baseEndurance = this.baseEndurance,
                baseTenacity = this.baseTenacity
            };
        }
    }
}
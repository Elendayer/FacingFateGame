using System.Collections.Generic;
using UnityEngine;

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
            RegisterNpc(new NpcData()
            {
                id = "Striker",
                name = "Striker",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),

                cardIds = new List<string>()
                {
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                    "Neutral_Tech_Strike",
                }
            });

            RegisterNpc(new NpcData()
            {
                id = "Healer",
                name = "Healer",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck "),

                cardIds = new List<string>()
                {
                    "Neutral_Abil_Recover",
                    "Neutral_Abil_Recover"
                }
            });

            RegisterNpc(new NpcData()
            {
                id = "Spear_Test_Npc",
                name = "Spearman",
                aiBias = AiBiasDatabase.GetBiasById("StupidFuck"),

                cardIds = new List<string>()
            {
                    "Neutral_Tech_Strike",
                    "Spear_Tech_Tempest_of_a_Hundred_Spears",
                    "Spear_Tech_Piercing_Light",
                    "Spear_Tech_Sky_Piercing_Leap",
                     "Spear_Tech_Heaven_Piercing_Spear",
                     "Spear_Tech_Salamander_Tail_Sweep",
                     "Spear_Tech_Snake_Tail_Sweep",
                     "Spear_Tech_Dragon_Tail_Sweep",
                     "Spear_Tech_Earthshatter_Pole",
                     "Spear_Tech_Azure_Dragons_Roar",
                     "Spear_Tech_Pillar_of_the_Earth",
                     "Spear_Abil_Extending_Heavens_Lance",
                     "Spear_Abil_Iron_Wall_Reversal",
                     "Spear_Abil_Whirling_Heaven_Ward",
                     "Spear_Abil_Unyielding_Spear_Stance",
                     //"Spear_Abil_Sky_Rending_Reversal",
                     "Spear_Abil_Phalanx_Guard",
                    "Spear_Curse_Brittle_Courage",
                    "Spear_Bless_Brilliant_Spear",

                }
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
            }
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
        }
    }

    public class NpcData
    {
        public string id;
        public string name;
        public NpcAiBias aiBias = new();

        public List<string> cardIds = new();

        public NpcData Clone(EntityScript entity)
        {
            return new NpcData
            {
                id = this.id,
                name = this.name,
                aiBias = this.aiBias,
                cardIds = this.cardIds
            };
        }
    }
}
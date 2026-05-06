using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    public static class CardSoundHelper
    {
        // Default sound — Strike card values. Applied first, card-specific switches override per group.
        private const string DefaultEvent = "Play_Card_SFX";

        private static readonly List<WwiseSwitchEntry> DefaultSwitches = new()
        {
            new WwiseSwitchEntry { group = "ActionType",         value = "Attack" },
            new WwiseSwitchEntry { group = "WeaponType",         value = "Fist"   },
            new WwiseSwitchEntry { group = "ElementType",        value = "Blood"  },
            new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"  },
        };

        /// <summary>
        /// Always sets all default switches first (Strike values), then overrides with
        /// card-specific switches. Posts playSfxEvent, or default event if not set.
        /// Every switch group always has a value → no Wwise "no default" errors.
        /// </summary>
        public static void PlayCardEffect(CardData card, GameObject emitter)
        {
            if (emitter == null) return;

            // 1) Set defaults so every switch group has a value
            foreach (var sw in DefaultSwitches)
                AkUnitySoundEngine.SetSwitch(sw.group, sw.value, emitter);

            // 2) Override with card-specific switches
            if (card?.soundSwitches != null)
            {
                foreach (var sw in card.soundSwitches)
                {
                    if (string.IsNullOrEmpty(sw.group) || string.IsNullOrEmpty(sw.value)) continue;
                    AkUnitySoundEngine.SetSwitch(sw.group, sw.value, emitter);
                }
            }

            // 3) Post event
            string eventName = (card != null && !string.IsNullOrEmpty(card.playSfxEvent))
                ? card.playSfxEvent
                : DefaultEvent;

            AkUnitySoundEngine.PostEvent(eventName, emitter);
        }
    }
}

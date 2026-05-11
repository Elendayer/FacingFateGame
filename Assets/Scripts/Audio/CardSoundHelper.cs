using System.Collections.Generic;
using UnityEngine;

namespace facingfate
{
    public static class CardSoundHelper
    {
        private const string DefaultEventName = "PlayCardSFX";
        private const string DotTickEventName = "DotTickSfx";

        private static readonly List<WwiseSwitchEntry> DefaultSwitches = new()
        {
            new WwiseSwitchEntry { group = "ActionType",         value = "Attack" },
            new WwiseSwitchEntry { group = "WeaponType",         value = "Fist"   },
            new WwiseSwitchEntry { group = "ElementType",        value = "Blood"  },
            new WwiseSwitchEntry { group = "SwitchGrp_CharType", value = "Human"  },
        };

        // Switch overrides per DoT type — only the groups that differ from defaults need to be listed
        private static readonly Dictionary<GameplayRef, List<WwiseSwitchEntry>> DotSwitches = new()
        {
            [GameplayRef.onBleed]  = new() { new WwiseSwitchEntry { group = "ActionType", value = "DoT" }, new WwiseSwitchEntry { group = "ElementType", value = "Blood"  } },
            [GameplayRef.onPoison] = new() { new WwiseSwitchEntry { group = "ActionType", value = "DoT" }, new WwiseSwitchEntry { group = "ElementType", value = "Poison" } },
            [GameplayRef.onBurn]   = new() { new WwiseSwitchEntry { group = "ActionType", value = "DoT" }, new WwiseSwitchEntry { group = "ElementType", value = "Fire"   } },
        };

        /// <summary>
        /// Always sets all default switches first (Strike values), then overrides with
        /// card-specific switches. Posts playSfxEvent, or default event if not set.
        /// Every switch group always has a value → no Wwise "no default" errors.
        /// Uses hard event references from AudioManager for type-safety and validation.
        /// </summary>
        public static void PlayCardEffect(CardData card, GameObject emitter)
        {
            if (emitter == null) return;

            // Ensure AudioManager exists
            if (AudioManager.Instance == null)
            {
                Debug.LogError("AudioManager not found in scene. Please add an AudioManager to your scene.");
                return;
            }

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

            // 3) Post event using AudioManager for hard references
            if (card != null && !string.IsNullOrEmpty(card.playSfxEvent))
            {
                var eventRef = AudioManager.Instance.GetEvent(card.playSfxEvent);
                if (eventRef != null && eventRef.IsValid())
                {
                    eventRef.Post(emitter);
                }
                else
                {
                    Debug.LogWarning($"Card event '{card.playSfxEvent}' not found in AudioManager. Falling back to default.");
                    PostDefaultEvent(emitter);
                }
            }
            else
            {
                PostDefaultEvent(emitter);
            }
        }

        private static void PostDefaultEvent(GameObject emitter)
        {
            var defaultEvent = AudioManager.Instance.GetEvent(DefaultEventName);
            if (defaultEvent != null && defaultEvent.IsValid())
            {
                defaultEvent.Post(emitter);
            }
            else
            {
                Debug.LogWarning($"Default Wwise event '{DefaultEventName}' not found in AudioManager registry.");
            }
        }

        /// <summary>
        /// Plays a DoT tick sound using Wwise switches to differentiate Bleed, Poison, and Burn.
        /// Sets defaults first, then overrides with the DoT-specific switches.
        /// </summary>
        public static void PlayDoTTick(GameplayRef dotType, GameObject emitter)
        {
            if (emitter == null) return;
            if (AudioManager.Instance == null)
            {
                Debug.LogError("AudioManager not found in scene.");
                return;
            }

            // 1) Set defaults
            foreach (var sw in DefaultSwitches)
                AkUnitySoundEngine.SetSwitch(sw.group, sw.value, emitter);

            // 2) Override with DoT-specific switches
            if (DotSwitches.TryGetValue(dotType, out var overrides))
            {
                foreach (var sw in overrides)
                    AkUnitySoundEngine.SetSwitch(sw.group, sw.value, emitter);
            }

            // 3) Post the DoT tick event
            var eventRef = AudioManager.Instance.GetEvent(DotTickEventName);
            if (eventRef != null && eventRef.IsValid())
                eventRef.Post(emitter);
            else
                Debug.LogWarning($"DoT tick event '{DotTickEventName}' not found in AudioManager.");
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace facingfate
{
    /// <summary>
    /// Unified Wwise event registry for the game.
    /// Provides centralized, hard references to all Wwise events used throughout the game.
    /// Prevents "Event ID not found" errors by using inspector-assigned event references.
    /// 
    /// Access via: AudioManager.Instance.GetEvent("EventName")
    ///           or AudioManager.Instance.PostEvent("EventName", emitter)
    /// 
    /// Event handling is done separately in CombatAudioController and CardSoundHelper.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [System.Serializable]
        public struct WwiseEventEntry
        {
            [Tooltip("Unique name key for this event (e.g., 'PlayCardSFX', 'PlayAttackSound')")]
            public string eventName;

            [Tooltip("Hard reference to the Wwise event")]
            public AK.Wwise.Event eventReference;
        }

        [Header("Common Card Events")]
        [Tooltip("Default card effect sound")] public AK.Wwise.Event playCardSFX;
        [Tooltip("Card draw/hand management sound")] public AK.Wwise.Event playCardDrawSound;
        [Tooltip("Card discard sound")] public AK.Wwise.Event playCardDiscardSound;

        [Header("Common Combat Events")]
        [Tooltip("Combat start sound")] public AK.Wwise.Event combatStartSfx;
        [Tooltip("Turn start sound")] public AK.Wwise.Event turnStartSfx;
        [Tooltip("Round start sound")] public AK.Wwise.Event roundStartSfx;
        [Tooltip("Victory/win sound")] public AK.Wwise.Event victorySfx;
        [Tooltip("Defeat/lose sound")] public AK.Wwise.Event defeatSfx;

        [Header("Common Impact Events")]
        [Tooltip("Generic attack/strike sound")] public AK.Wwise.Event playAttackSound;
        [Tooltip("Hit/impact sound")] public AK.Wwise.Event playHitSound;
        [Tooltip("Damage taken sound")] public AK.Wwise.Event playDamageSound;

        [Header("Wwise Events Registry")]
        [SerializeField] private List<WwiseEventEntry> registeredEvents = new();

        private Dictionary<string, AK.Wwise.Event> eventDict;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Build dictionary for O(1) lookup
            eventDict = new Dictionary<string, AK.Wwise.Event>();

            // Register common card events
            RegisterEvent("PlayCardSFX", playCardSFX);
            RegisterEvent("PlayCardDrawSound", playCardDrawSound);
            RegisterEvent("PlayCardDiscardSound", playCardDiscardSound);

            // Register common combat flow events
            RegisterEvent("CombatStartSfx", combatStartSfx);
            RegisterEvent("TurnStartSfx", turnStartSfx);
            RegisterEvent("RoundStartSfx", roundStartSfx);
            RegisterEvent("VictorySfx", victorySfx);
            RegisterEvent("DefeatSfx", defeatSfx);

            // Register common impact events
            RegisterEvent("PlayAttackSound", playAttackSound);
            RegisterEvent("PlayHitSound", playHitSound);
            RegisterEvent("PlayDamageSound", playDamageSound);

            // Register any additional custom events from the list
            foreach (var entry in registeredEvents)
            {
                RegisterEvent(entry.eventName, entry.eventReference);
            }

            if (eventDict.Count == 0)
            {
                Debug.LogWarning("AudioManager: No valid Wwise events registered. Please assign events in the Inspector.");
            }
            else
            {
                Debug.Log($"AudioManager initialized with {eventDict.Count} Wwise events.");
            }
        }

        /// <summary>
        /// Registers an event to the internal dictionary for lookup.
        /// </summary>
        private void RegisterEvent(string eventName, AK.Wwise.Event eventReference)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (eventReference == null || !eventReference.IsValid())
            {
                Debug.LogWarning($"AudioManager: Event '{eventName}' has invalid or null reference. Skipping.");
                return;
            }

            if (eventDict.ContainsKey(eventName))
            {
                Debug.LogWarning($"AudioManager: Event '{eventName}' is already registered. Using first occurrence.");
                return;
            }

            eventDict.Add(eventName, eventReference);
        }

        #region Event Registry API

        /// <summary>
        /// Retrieves a Wwise event by name.
        /// </summary>
        /// <param name="eventName">The event name key (must match exactly)</param>
        /// <returns>The AK.Wwise.Event reference, or null if not found</returns>
        public AK.Wwise.Event GetEvent(string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning("AudioManager.GetEvent: eventName is null or empty.");
                return null;
            }

            if (eventDict.TryGetValue(eventName, out var eventRef))
            {
                return eventRef;
            }

            Debug.LogWarning($"AudioManager: Event '{eventName}' not found in registry.");
            return null;
        }

        /// <summary>
        /// Checks if an event exists in the registry.
        /// </summary>
        public bool EventExists(string eventName)
        {
            return !string.IsNullOrEmpty(eventName) && eventDict.ContainsKey(eventName);
        }

        /// <summary>
        /// Posts a Wwise event by name on a GameObject.
        /// </summary>
        /// <param name="eventName">The event name key</param>
        /// <param name="emitter">The GameObject to emit the sound from</param>
        /// <returns>The playing ID, or AK_INVALID_PLAYING_ID if event not found</returns>
        public uint PostEvent(string eventName, GameObject emitter)
        {
            if (emitter == null)
            {
                Debug.LogWarning($"AudioManager.PostEvent: emitter is null for event '{eventName}'.");
                return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
            }

            var eventRef = GetEvent(eventName);
            if (eventRef != null)
            {
                return eventRef.Post(emitter);
            }

            return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
        }

        /// <summary>
        /// Posts a Wwise event directly by reference.
        /// </summary>
        public uint PostEvent(AK.Wwise.Event eventRef, GameObject emitter)
        {
            if (eventRef == null || !eventRef.IsValid())
            {
                return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
            }

            if (emitter == null)
            {
                Debug.LogWarning("AudioManager.PostEvent: emitter is null.");
                return AkUnitySoundEngine.AK_INVALID_PLAYING_ID;
            }

            return eventRef.Post(emitter);
        }

        /// <summary>
        /// Returns all registered event names (for debugging/validation).
        /// </summary>
        public List<string> GetAllEventNames()
        {
            return eventDict.Keys.ToList();
        }

        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only validation to check for missing or invalid references.
        /// </summary>
        private void OnValidate()
        {
            // Check common fields for null references (only warn for critical ones)
            if (playCardSFX == null)
                Debug.LogWarning("AudioManager: 'playCardSFX' is not assigned. Please assign a Wwise event.", this);

            // Check for duplicates in custom registry
            var names = new HashSet<string>();
            foreach (var entry in registeredEvents)
            {
                if (!string.IsNullOrEmpty(entry.eventName))
                {
                    if (names.Contains(entry.eventName))
                    {
                        Debug.LogWarning($"AudioManager: Duplicate event name '{entry.eventName}' detected in registry.", this);
                    }
                    names.Add(entry.eventName);
                }
            }

            // Check for null references in custom registry
            foreach (var entry in registeredEvents)
            {
                if (!string.IsNullOrEmpty(entry.eventName) && entry.eventReference == null)
                {
                    Debug.LogWarning($"AudioManager: Event '{entry.eventName}' has a null reference. Please assign a Wwise event.", this);
                }
            }
        }
#endif
    }
}

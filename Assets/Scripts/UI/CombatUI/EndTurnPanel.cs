using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace facingfate
{
    public class EndTurnPanel : MonoBehaviour
    {
        [SerializeField] private Button endTurnButton;

        private void Awake()
        {
            if (endTurnButton != null) endTurnButton.onClick.AddListener(EndTurn);
        }

        public void RefreshInteractable(Component currentTurnEntity)
        {
            if (endTurnButton == null) return;

            // Minimal: always interactable (später: nur wenn Player am Zug)
            endTurnButton.interactable = true;
        }

        private void EndTurn()
        {
            // Try EventManager.Instance.Endturn()
            Type eventManagerType = ReflectionUtility.FindTypeByName("EventManager");
            if (eventManagerType != null)
            {
                object instance = ReflectionUtility.TryGetStaticFieldOrProperty(eventManagerType, "Instance");
                if (instance != null)
                {
                    MethodInfo m = instance.GetType().GetMethod("Endturn", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                 ?? instance.GetType().GetMethod("EndTurn", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                    if (m != null)
                    {
                        m.Invoke(instance, null);
                        return;
                    }
                }
            }

            // Fallback: GameEvents.TriggerTurnEnd()
            Type gameEventsType = ReflectionUtility.FindTypeByName("GameEvents");
            if (gameEventsType != null)
            {
                MethodInfo m = gameEventsType.GetMethod("TriggerTurnEnd", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (m != null)
                {
                    m.Invoke(null, null);
                }
            }
        }
    }
}

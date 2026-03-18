using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace facingfate
{
    public class PlayerStatsPanel : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text hpText;
        [SerializeField] private Slider hpSlider;

        [SerializeField] private TMP_Text staminaText;
        [SerializeField] private StatusEffectBarUI statusBar;

        private Component boundEntity;

        public void Bind(Component entity)
        {
            boundEntity = entity;
            Debug.Log($"[PlayerStatPanel] Bind – entity={entity?.gameObject.name ?? "NULL"}");
            if (statusBar != null) statusBar.Bind(entity);
            else Debug.Log("[PlayerStatPanel] statusBar ist NULL!");
        }

        public void Refresh()
        {
            Debug.Log($"[PlayerStatPanel] Refresh – boundEntity={boundEntity?.gameObject.name ?? "NULL"}, statusBar={statusBar?.name ?? "NULL"}");
            if (boundEntity == null)
            {
                SetText(nameText, "-"); 
                SetText(hpText, "-"); 
                SetText(staminaText, "-");
                SetSlider(hpSlider, 0f, 1f);
                statusBar?.Refresh();
                return;
            }

            SetText(nameText, GetEntityName(boundEntity));

            if (EntityStatReader.TryGetHealth(boundEntity, out int hpCur, out int hpMax))
            {
                SetText(hpText, hpMax > 0f ? $"{hpCur:0}/{hpMax:0}"+" HP" : $"{hpCur:0}/??");
                SetSlider(hpSlider, hpCur, hpMax);
            }
            else { SetText(hpText, "-"); SetSlider(hpSlider, 0f, 1f); }

            if (EntityStatReader.TryGetStamina(boundEntity, out int stCur, out int stMax))
                SetText(staminaText, stMax > 0f ? $"{stCur:0}/{stMax:0}"+" Stamina" : $"{stCur:0}/??");
            else
                SetText(staminaText, "-");

            statusBar?.Refresh();
        }

        private static void SetText(TMP_Text t, string s)
        {
            if (t == null) return;
            if (t.text != s) t.text = s;
        }
        private static void SetSlider(Slider s, float current, float max)
        {
            if (s == null) return;
            s.minValue = 0f;
            s.maxValue = max > 0f ? max : 1f;
            s.value = current >= 0f ? current : 0f;
        }
        private static string GetEntityName(Component entity)
        {
            // Versucht zuerst NpcData.name über NonPlayerScript zu lesen
            object npcData = ReflectionUtility.TryGetFieldOrProperty(entity, "npcData")
                          ?? ReflectionUtility.TryGetFieldOrProperty(entity, "NpcData");

            if (npcData != null)
            {
                object nameObj = ReflectionUtility.TryGetFieldOrProperty(npcData, "name");
                if (nameObj != null && !string.IsNullOrWhiteSpace(nameObj.ToString()))
                    return nameObj.ToString();
            }

            // Fallback: GameObject-Name
            return entity.gameObject.name;
        }
    }
}

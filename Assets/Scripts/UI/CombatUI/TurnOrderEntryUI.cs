using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace facingfate
{
    public class TurnOrderEntryUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private Image background;
        private EntityScript boundEntity;

        public void Setup(string entityName, Color color, bool isCurrent, EntityScript entity)
        {
            boundEntity = entity;
            if (nameText != null)
            {
                nameText.text = entityName;
                nameText.fontStyle = isCurrent ? FontStyles.Bold : FontStyles.Normal;
                // Aktueller Zug etwas größer
                nameText.fontSize = isCurrent ? 16f : 13f;
            }

            if (background != null)
                background.color = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (boundEntity == null) return;
            boundEntity.GetComponent<EntityOutline>()?.SetHover(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (boundEntity == null) return;
            boundEntity.GetComponent<EntityOutline>()?.SetNormal();
        }
    }
}

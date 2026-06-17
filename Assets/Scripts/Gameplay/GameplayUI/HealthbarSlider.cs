using TMPro;
using UnityEngine;

namespace facingfate
{
    public class HealthbarSlider : MonoBehaviour
    {
        public TextMeshProUGUI textMeshPro;
        public EntityScript eM;

        public MeshRenderer healthRenderer;
        public MeshRenderer staminaRenderer;

        private void Start()
        {
            eM = GetComponentInParent<EntityScript>();
        }

        private void Update()
        {
            if (eM == null) return;

            float maxHp = eM.entityStats.MaxHealth;
            float maxSt = eM.entityStats.MaxStamina;

            if (healthRenderer != null)
                healthRenderer.material.SetFloat("_ratio", maxHp > 0f ? eM.entityStats.CurrentHealth / maxHp : 0f);

            if (staminaRenderer != null)
                staminaRenderer.material.SetFloat("_ratio", maxSt > 0f ? eM.entityStats.CurrentStamina / maxSt : 0f);
        }
    }
}
using System.Collections;
using TMPro;
using UnityEngine;

namespace facingfate
{
    public class HealthbarSlider : MonoBehaviour
    {
        public TextMeshProUGUI textMeshPro;
        public EntityScript eM;

        float maxHealth => eM.entityStats.MaxHealth.Value();
        float currentHealth => eM.entityStats.CurrentHealth;

        float maxStamina => eM.entityStats.MaxStamina.Value();
        float currentStamina => eM.entityStats.CurrentStamina;

        public MeshRenderer healthRenderer;
        public MeshRenderer staminaRenderer;


        private void Start()
        {
            eM = GetComponentInParent<EntityScript>();

            MeshRenderer renderer = GetComponent<MeshRenderer>();

            StartCoroutine(SlowUpdate());
        }

        // Update is called once per frame
        IEnumerator SlowUpdate()
        {
            while (true)
            {
                if (currentHealth != 0)
                {
                    healthRenderer.material.SetFloat("_ratio", currentHealth / maxHealth);
                    staminaRenderer.material.SetFloat("_ratio", currentStamina / maxStamina);
                    //textMeshPro.text = $"{current} / {max}"; 
                }
                yield return new WaitForSeconds(0.5f);
            }
        }
    }
}
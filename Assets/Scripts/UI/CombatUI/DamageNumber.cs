using UnityEngine;
using TMPro;
using DG.Tweening;

namespace facingfate
{
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float floatDistance = 1.5f;
        [SerializeField] private float duration = 1.2f;
        [SerializeField] private float randomOffsetX = 0.3f;

        public void Play(string value, Color color)
        {
            if (text != null)
            {
                text.text = value;
                text.color = color;
            }

            float randomX = Random.Range(-randomOffsetX, randomOffsetX);
            Vector3 targetPos = transform.position + new Vector3(randomX, floatDistance, 0f);

            // Nach oben fliegen
            transform.DOMove(targetPos, duration)
                .SetEase(Ease.OutQuart);

            // Ausfaden + zerstören
            text.DOFade(0f, duration)
                .SetEase(Ease.InQuart)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
using UnityEngine;
using TMPro;
using DG.Tweening;

namespace facingfate
{
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private float floatDistance  = 5f;
        [SerializeField] private float duration       = 1.2f;
        [SerializeField] private float randomOffsetX  = 0.4f;
        [SerializeField] private float textScale      = 2.5f;

        public void Play(string value, Color color)
        {
            if (text != null)
            {
                text.text  = value;
                text.color = color;
            }

            // Scale up the number
            transform.localScale = Vector3.one * textScale;

            // Float toward camera — use camera forward direction so numbers drift into view
            Vector3 towardCam = Camera.main != null
                ? -Camera.main.transform.forward.normalized
                : Vector3.up;

            float randomX = Random.Range(-randomOffsetX, randomOffsetX);
            Vector3 targetPos = transform.position
                + towardCam * floatDistance
                + new Vector3(randomX, 0f, 0f);

            transform.DOMove(targetPos, duration)
                .SetEase(Ease.OutQuart);

            text.DOFade(0f, duration)
                .SetEase(Ease.InQuart)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace facingfate
{

    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance { get; private set; }

        [SerializeField] private RectTransform root;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text bodyText;

        [Header("Fade Settings")]
        //[SerializeField] private float displayDuration = 2f; 
        [SerializeField] private float fadeDuration = 0.5f;

        private CanvasGroup canvasGroup;
        private Coroutine fadeCoroutine;

        private void Awake()
        {
            Instance = this;

            canvasGroup = root.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = root.gameObject.AddComponent<CanvasGroup>();
            
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            Hide();
        }

        public void Show(string header, string body)
        {
            if (root == null) return;

            bool wasVisible = root.gameObject.activeSelf && canvasGroup.alpha > 0f;

            if (headerText != null) headerText.text = header ?? "";
            if (bodyText != null) bodyText.text = body ?? "";

            // Fade stoppen und sofort voll sichtbar machen
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }

            root.gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            if (!wasVisible)
            {
                FollowMouse();
            }
        }

        public void Hide()
        {
            if (root == null) return;

            // Fade starten statt sofort verstecken
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeOut());
        }

        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            root.gameObject.SetActive(false);
            fadeCoroutine = null;
        }

        private void FollowMouse()
        {
            if (root == null) return;

            Canvas.ForceUpdateCanvases();

            float w = root.rect.width;
            float h = root.rect.height;
            float margin = 20f;
            Vector2 mouse = Input.mousePosition;

            // Untere linke Ecke = Mausposition
            float x = mouse.x + margin;
            float y = mouse.y + margin;

            // Rechter Rand
            if (x + w > Screen.width - margin)
                x = mouse.x - w - margin;

            // Oberer Rand
            if (y + h > Screen.height - margin)
                y = Screen.height - h - margin;

            // Linker Rand
            if (x < margin)
                x = margin;

            // Unterer Rand
            if (y < margin)
                y = margin;

            root.position = new Vector2(x, y);
        }
    }
}

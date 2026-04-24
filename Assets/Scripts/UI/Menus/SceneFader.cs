using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace facingfate
{
    public class SceneFader : MonoBehaviour
    {
        public static SceneFader Instance { get; private set; }

        [SerializeField] private CanvasGroup overlay;
        [SerializeField] private float fadeDuration = 0.5f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            overlay.alpha = 1f;
            overlay.DOFade(0f, fadeDuration).SetEase(Ease.OutQuart);
        }

        public void FadeToScene(string sceneName)
        {
            overlay.DOKill();
            overlay.DOFade(1f, fadeDuration).SetEase(Ease.InQuart)
                .OnComplete(() => SceneManager.LoadScene(sceneName));
        }
    }
}

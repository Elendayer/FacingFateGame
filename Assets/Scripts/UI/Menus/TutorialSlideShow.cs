using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using DG.Tweening;
using System;
//using System.Drawing.Printing;
using UnityEngine.SceneManagement;
//using FMODUnity;

namespace facingfate
{
    public class TutorialSlideShow : MonoBehaviour
    {
        [System.Serializable]
        public class TutorialPage
        {
            public LocalizedString localizedTitle;
            public LocalizedString localizedDescription;
            public LocalizedSprite localizedImage;

            public GameObject additionalUI; // z. B. Button, Pfeile, Steuerungshinweise
        }

        [Header("UI References")]
        public CanvasGroup canvasGroup;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public Image illustrationImage;
        public TextMeshProUGUI progressText;

        [Header("Tutorial Pages")]
        public TutorialPage[] pages;
        private int currentPageIndex = 0;

        [Header("Skip Settings")]
        public Image SkipImage;
        public Image skipProgressBar;
        public float skipHoldDuration = 2f;
        private float skipHoldTime = 0f;
        private bool isSkipPressed = false;
        private Tween rotationTween;
        private bool rotationStarted = false;

        [Header("Scene Transition")]
        public bool transitionOnClose = false;
        [Tooltip("Index aus Build Settings")]
        public int targetSceneIndex = -1;

        private const float PageFadeDuration = 0.5f;
        private const float TutorialCloseFadeDuration = 1f;
        public float rotationSpeed = 100f;

        //private DungeonPhaseInput inputActions;

        private bool isTransitioning = false;

        //public EventReference flipPagesEvent;

        private void Awake()
        {
            /*
            inputActions = new DungeonPhaseInput();

            inputActions.CardPhase.RotateR.performed += ctx => NextPage();
            inputActions.CardPhase.RotateL.performed += ctx => PreviousPage();
            inputActions.CardPhase.Pause.performed += ctx => CancelTutorial();
            inputActions.CardPhase.Submit.started += ctx => isSkipPressed = true;
            inputActions.CardPhase.Submit.canceled += ctx =>
            {
                isSkipPressed = false;
                skipHoldTime = 0f;
                UpdateSkipBar(0f);

                if (rotationTween != null && rotationTween.IsActive())
                {
                    rotationTween.Kill();
                }

                SkipImage.transform
                    .DORotate(Vector3.zero, 0.3f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true);

                rotationStarted = false;
            };
            inputActions.CardPhase.RotateR.started += ctx => isSkipPressed = true;
            inputActions.CardPhase.RotateR.canceled += ctx =>
            {
                isSkipPressed = false;
                skipHoldTime = 0f;
                UpdateSkipBar(0f);

                if (rotationTween != null && rotationTween.IsActive())
                {
                    rotationTween.Kill();
                }

                SkipImage.transform
                    .DORotate(Vector3.zero, 0.3f)
                    .SetUpdate(true)
                    .SetEase(Ease.OutCubic)
                    .SetUpdate(true);

                rotationStarted = false;
            };
            */
        }


        private void Update()
        {
            if (isSkipPressed)
            {
                if (!rotationStarted)
                {
                    rotationStarted = true;
                    rotationTween = SkipImage.transform
                        .DORotate(new Vector3(0, 0, -360f), skipHoldDuration, RotateMode.FastBeyond360)
                        .SetUpdate(true)
                        .SetEase(Ease.Linear)
                        .SetUpdate(true);
                }

                skipHoldTime += Time.unscaledDeltaTime;
                UpdateSkipBar(skipHoldTime / skipHoldDuration);

                if (skipHoldTime >= skipHoldDuration)
                {
                    rotationTween.Kill(); // Falls noch aktiv
                    CloseTutorial();
                }
            }
        }


        void UpdateSkipBar(float progress)
        {
            if (skipProgressBar != null)
                skipProgressBar.fillAmount = Mathf.Clamp01(progress);
        }

        public void NextPage()
        {
            if (isTransitioning || currentPageIndex >= pages.Length - 1) return;
            currentPageIndex++;
            ShowPage(currentPageIndex);
        }

        public void PreviousPage()
        {
            if (isTransitioning || currentPageIndex <= 0) return;
            currentPageIndex--;
            ShowPage(currentPageIndex);
        }

        void ShowPage(int index, bool instant = false)
        {
            if (pages.Length == 0 || index >= pages.Length) return;

            //RuntimeManager.PlayOneShot(flipPagesEvent, transform.position);
            if (instant)
            {
                ApplyPage(index);
                canvasGroup.DOFade(0, PageFadeDuration).SetUpdate(true).OnComplete(() =>
                {
                    canvasGroup.DOFade(1, PageFadeDuration).SetUpdate(true);
                });

            }
            else
            {
                isTransitioning = true;
                canvasGroup.DOFade(0, PageFadeDuration).SetUpdate(true).OnComplete(() =>
                {
                    ApplyPage(index);
                    canvasGroup.DOFade(1, PageFadeDuration).SetUpdate(true).OnComplete(() =>
                    {
                        isTransitioning = false;
                    });
                });
            }

            UpdateProgressDisplay();
        }

        void ApplyPage(int index)
        {
            var page = pages[index];

            foreach (var p in pages)
            {
                if (p.additionalUI != null)
                    p.additionalUI.SetActive(false);
            }

            page.localizedTitle.StringChanged += value => titleText.text = value;
            page.localizedDescription.StringChanged += value => descriptionText.text = value;
            page.localizedTitle.RefreshString();
            page.localizedDescription.RefreshString();

            if (illustrationImage != null && page.localizedImage != null)
            {
                page.localizedImage.LoadAssetAsync().Completed += handle =>
                {
                    if (illustrationImage != null && illustrationImage.isActiveAndEnabled)
                    {
                        illustrationImage.sprite = handle.Result;
                        illustrationImage.enabled = (handle.Result != null);
                    }
                };
            }

            if (page.additionalUI != null)
            {
                page.additionalUI.SetActive(true);
            }
        }

        void UpdateProgressDisplay()
        {
            if (progressText != null)
                progressText.text = $"{currentPageIndex + 1} / {pages.Length}";
        }

        void CloseTutorial()
        {
            canvasGroup.DOFade(0, TutorialCloseFadeDuration).SetUpdate(true).OnComplete(() =>
            {
                ResetTutorial();

                gameObject.SetActive(false);

                //DdCodeEventHandler.Trigger_TutorialDone();

                if (transitionOnClose)
                {
                    SceneManager.LoadScene(targetSceneIndex);
                }


            });
        }

        void CancelTutorial()
        {
            canvasGroup.DOFade(0, TutorialCloseFadeDuration).SetUpdate(true).OnComplete(() =>
            {
                ResetTutorial();

                gameObject.SetActive(false);

                //DdCodeEventHandler.Trigger_TutorialCancel();
            });
        }

        void ResetTutorial()
        {
            currentPageIndex = 0;
            skipHoldTime = 0f;
            isSkipPressed = false;
            isTransitioning = false;

            UpdateSkipBar(0f);
            ShowPage(currentPageIndex, instant: true);
        }
        private void OnEnable()
        {
            //inputActions.Enable();
            //DdCodeEventHandler.TutorialDone += CloseTutorial;
            canvasGroup.alpha = 0f;
            ShowPage(currentPageIndex, instant: true);
        }

        private void OnDisable()
        {
            //DdCodeEventHandler.TutorialDone -= CloseTutorial;
            //inputActions.Disable();
        }

    }
}

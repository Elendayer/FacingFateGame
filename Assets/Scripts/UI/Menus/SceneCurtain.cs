using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
//using FMODUnity;
//using FMOD.Studio;

namespace facingfate
{
    public class SceneCurtain : MonoBehaviour
    {
        public RectTransform topPanel;
        private float topPanelPosY = 1240f;
        public RectTransform bottomPanel;
        private float bottomPanelPosY = -1240f;
        public float animationDuration = 1f;
        public bool openGym;

        public GameObject tutorialSlideshow;

        //public EventReference courtainOpenEvent;

        [Tooltip("Index aus Build Settings")] public int targetSceneIndex = -1;

        protected virtual void Start()
        {
            if (openGym)
            {
                OpenCurtain();
            }
        }

        public void StartTransitionToScene()
        {
            topPanel.DOAnchorPosY(bottomPanelPosY, animationDuration).SetUpdate(true).SetEase(Ease.InOutSine);
            bottomPanel.DOAnchorPosY(topPanelPosY, animationDuration).SetUpdate(true).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    var showTutorial = FindFirstObjectByType<SetShowTutorial>();
                    if (showTutorial != null && showTutorial.showTutorial)
                    {
                        tutorialSlideshow?.SetActive(true);
                        return;
                    }
                    SceneManager.LoadScene(targetSceneIndex);
                });
        }

        public void StartTransitionToSceneByName(string sceneName)
        {
            topPanel.DOAnchorPosY(bottomPanelPosY, animationDuration).SetUpdate(true).SetEase(Ease.InOutSine);
            bottomPanel.DOAnchorPosY(topPanelPosY, animationDuration).SetUpdate(true).SetEase(Ease.InOutSine)
                .OnComplete(() => SceneManager.LoadScene(sceneName));
        }


        protected void OpenCurtain()
        {
            ChangeCurtainSingle(true, true);
            ChangeCurtainSingle(false, true);
        }

        protected void ChangeCurtainSingle(bool isTopPanel, bool open)
        {
            var panel = isTopPanel ? topPanel : bottomPanel;
            float targetY = isTopPanel
                ? (open ? topPanelPosY : bottomPanelPosY)
                : (open ? bottomPanelPosY : topPanelPosY);

            panel.DOAnchorPosY(targetY, animationDuration).SetUpdate(true).SetEase(Ease.InOutSine)
                .OnComplete(() =>
                {
                    if (!open) tutorialSlideshow?.SetActive(true);
                });
        }

        public void CloseCurtain()
        {
            ChangeCurtainSingle(true, false);
            ChangeCurtainSingle(false, false);
        }

        private void OnEnable()
        {
            //DdCodeEventHandler.TutorialDone += OpenCurtain;
        }

        private void OnDisable()
        {
            //DdCodeEventHandler.TutorialDone -= OpenCurtain;
        }
    }
}
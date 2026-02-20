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
                    if (FindFirstObjectByType<SetShowTutorial>().showTutorial)
                    {
                        tutorialSlideshow.SetActive(true);
                        return;
                    }
                    else
                    {
                        SceneManager.LoadScene(targetSceneIndex);
                    }
                });
        }


        protected void OpenCurtain()
        {
            ChangeCurtainSingle(true, true);
            ChangeCurtainSingle(false, true);
        }

        protected void ChangeCurtainSingle(bool player1, bool open)
        {
            if (player1)
            {
                topPanel.DOAnchorPosY(open ? topPanelPosY : bottomPanelPosY, animationDuration).SetUpdate(true)
                    .SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        if (!open)
                        {
                            tutorialSlideshow.SetActive(true);
                            //RuntimeManager.PlayOneShot(courtainOpenEvent, transform.position);
                        }
                    });
            }
            else
            {
                bottomPanel.DOAnchorPosY(open ? bottomPanelPosY : topPanelPosY, animationDuration).SetUpdate(true)
                    .SetEase(Ease.InOutSine).OnComplete(() =>
                    {
                        if (!open)
                        {
                            tutorialSlideshow.SetActive(true);
                            //RuntimeManager.PlayOneShot(courtainOpenEvent, transform.position);
                        }
                    });
                ;
            }
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
using UnityEngine;

namespace facingfate
{
    public class TutorialCancel : MonoBehaviour
    {
        public GameObject mainMenu;

        void MainMenuActivate()
        {
            mainMenu.SetActive(true);
        }
        private void OnEnable()
        {
            //DdCodeEventHandler.TutorialCancel += MainMenuActivate;
        }

        private void OnDisable()
        {
            //DdCodeEventHandler.TutorialCancel -= MainMenuActivate;
        }

    }
}

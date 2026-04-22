using UnityEngine;
using UnityEngine.SceneManagement;

namespace facingfate
{
    /// <summary>
    /// Minimal helper for wiring scene loads to UI Button onClick events in the Inspector.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}

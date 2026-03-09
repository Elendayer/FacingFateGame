using UnityEngine;
using System.Collections;

namespace Utility
{
    public class CoroutineRunner : MonoBehaviour
    {
        public static CoroutineRunner Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        public Coroutine StartCoroutineManaged(IEnumerator routine)
        {
            return StartCoroutine(routine);
        }

        public void StopCoroutineManaged(Coroutine coroutine)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }
    }
}

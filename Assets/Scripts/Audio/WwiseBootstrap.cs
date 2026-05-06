using UnityEngine;

namespace facingfate
{
    public static class WwiseBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            if (Object.FindFirstObjectByType<AkInitializer>() != null) return;

            var prefab = Resources.Load<GameObject>("WwiseGlobal");
            if (prefab == null)
            {
                Debug.LogWarning("WwiseBootstrap: WwiseGlobal prefab not found in Resources. Audio will not work.");
                return;
            }

            Object.DontDestroyOnLoad(Object.Instantiate(prefab));
        }
    }
}

using UnityEngine;

namespace facingfate
{
    public class AudioBankPreloader : MonoBehaviour
    {
        [SerializeField] private string[] bankNames = { "Play_Card_SFX" };

        private uint[] _bankIds;

        private void Awake()
        {
            _bankIds = new uint[bankNames.Length];
            for (int i = 0; i < bankNames.Length; i++)
                AkUnitySoundEngine.LoadBank(bankNames[i], out _bankIds[i]);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _bankIds.Length; i++)
                AkUnitySoundEngine.UnloadBank(_bankIds[i], System.IntPtr.Zero);
        }
    }
}

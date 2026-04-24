using UnityEngine;
using UnityEngine.EventSystems;

namespace facingfate
{
    // Drop on any panel or button. All fields optional — unassigned = silent.
    public class UiAudioBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [Tooltip("Optional, empty = silent")] [SerializeField] private AK.Wwise.Event openSfx;
        [Tooltip("Optional, empty = silent")] [SerializeField] private AK.Wwise.Event closeSfx;
        [Tooltip("Optional, empty = silent")] [SerializeField] private AK.Wwise.Event hoverSfx;
        [Tooltip("Optional, empty = silent")] [SerializeField] private AK.Wwise.Event clickSfx;

        [SerializeField] private GameObject sfxEmitter;
        private GameObject Emitter => sfxEmitter != null ? sfxEmitter : gameObject;

        private void OnEnable() => WwiseAudioHelper.PlayGlobal(openSfx, Emitter);

        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            WwiseAudioHelper.PlayGlobal(closeSfx, Emitter);
        }

        public void OnPointerEnter(PointerEventData eventData) => WwiseAudioHelper.PlayGlobal(hoverSfx, Emitter);
        public void OnPointerClick(PointerEventData eventData) => WwiseAudioHelper.PlayGlobal(clickSfx, Emitter);
    }
}

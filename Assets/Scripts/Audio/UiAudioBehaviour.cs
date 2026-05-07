using UnityEngine;
using UnityEngine.EventSystems;

namespace facingfate
{
    // Drop on any panel or button. All fields optional — unassigned = silent.
    public class UiAudioBehaviour : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
    {
        [SerializeField] private GameObject sfxEmitter;
        private GameObject Emitter => sfxEmitter != null ? sfxEmitter : gameObject;

        private void OnEnable() => AudioManager.Instance?.PostEvent("UiOpenSfx", Emitter);

        private void OnDisable()
        {
            if (!Application.isPlaying) return;
            AudioManager.Instance?.PostEvent("UiCloseSfx", Emitter);
        }

        public void OnPointerEnter(PointerEventData eventData) => AudioManager.Instance?.PostEvent("UiHoverSfx", Emitter);
        public void OnPointerClick(PointerEventData eventData) => AudioManager.Instance?.PostEvent("UiClickSfx", Emitter);
    }
}

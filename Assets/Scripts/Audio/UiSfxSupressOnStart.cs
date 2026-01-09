using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
//using FMODUnity;

namespace facingfate
{
    public class UiSfxSuppressOnStart : MonoBehaviour, IPointerEnterHandler, IDeselectHandler
    {
        //[SerializeField] private EventReference sfxEvent;

        [Tooltip("Bei Maus-Hover nach dem ersten Deselect abspielen.")]
        [SerializeField] private bool playOnHover = true;

        private bool _initialized;           // true, sobald Startzustand sicher erfasst wurde
        private bool _armed;                 // darf SFX spielen (erst nach erstem Fokus-Verlust)
        private GameObject _lastSelected;    // zuletzt selektiertes Objekt
        private int _lastPlayFrame = -1;     // Schutz vor Doppeltrigger im selben Frame

        private void OnEnable()
        {
            _initialized = false;
            _armed = false;                  // initial stumm, falls direkt vorselektiert
            _lastSelected = null;
            _lastPlayFrame = -1;
            StartCoroutine(CaptureInitialSelection());
        }

        private IEnumerator CaptureInitialSelection()
        {
            // 1–2 Frames warten, damit das EventSystem seine initiale Selektion gesetzt hat
            yield return null;
            yield return new WaitForEndOfFrame();

            var es = EventSystem.current;
            var initial = es != null ? es.currentSelectedGameObject : null;

            // Wenn NICHT dieser Button initial selektiert ist, sofort scharf schalten
            _armed = (initial != gameObject);
            _lastSelected = initial;
            _initialized = true;
        }

        private void Update()
        {
            if (!_initialized) return;

            var es = EventSystem.current;
            if (es == null) return;

            var current = es.currentSelectedGameObject;

            // 1) Sobald der Fokus VON DIESEM Button weg ist, „armen“
            if (!_armed && _lastSelected == gameObject && current != gameObject)
            {
                _armed = true;
            }

            // 2) Controller/Keyboard: auf diesen Button gewechselt?
            if (_armed && current == gameObject && _lastSelected != gameObject)
            {
                PlayOncePerFrame();
            }

            _lastSelected = current;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (!_initialized) return;
            _armed = true; // zusätzlicher Sicherheitsgurt
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_initialized || !_armed || !playOnHover) return;
            PlayOncePerFrame();
        }

        private void PlayOncePerFrame()
        {
            if (_lastPlayFrame == Time.frameCount) return;
            _lastPlayFrame = Time.frameCount;

            /*
            if (!sfxEvent.IsNull)
            {
                RuntimeManager.PlayOneShot(sfxEvent, transform.position);
            }
            */
        }
    }
}

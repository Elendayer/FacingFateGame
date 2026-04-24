using UnityEngine;

namespace facingfate
{
    public static class WwiseAudioHelper
    {
        // Posts at emitter world position. Null/unlinked event or destroyed emitter = silent.
        public static void Play(AK.Wwise.Event wwiseEvent, GameObject emitter)
        {
            if (wwiseEvent == null || !wwiseEvent.IsValid() || emitter == null || !emitter) return;
            wwiseEvent.Post(emitter);
        }

        // Posts without 3D position. Falls back to Camera.main if no emitter given.
        public static void PlayGlobal(AK.Wwise.Event wwiseEvent, GameObject fallback = null)
        {
            if (wwiseEvent == null || !wwiseEvent.IsValid()) return;
            GameObject emitter = (fallback != null && fallback) ? fallback : Camera.main?.gameObject;
            if (emitter == null) return;
            wwiseEvent.Post(emitter);
        }
    }
}

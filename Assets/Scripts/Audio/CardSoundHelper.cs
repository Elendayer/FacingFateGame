using UnityEngine;

namespace facingfate
{
    public static class CardSoundHelper
    {
        /// <summary>
        /// Sets Wwise switches defined on the card, then posts the card's playSfxEvent.
        /// Silent if playSfxEvent is empty. No errors if switches are missing/empty.
        /// </summary>
        /// <param name="card">CardData of the played card.</param>
        /// <param name="emitter">GameObject to post the event on (use caster GO).</param>
        public static void PlayCardEffect(CardData card, GameObject emitter)
        {
            if (card == null || string.IsNullOrEmpty(card.playSfxEvent)) return;
            if (emitter == null) return;

            // Set each switch on the emitter before posting
            if (card.soundSwitches != null)
            {
                foreach (var sw in card.soundSwitches)
                {
                    if (string.IsNullOrEmpty(sw.group) || string.IsNullOrEmpty(sw.value)) continue;
                    AkUnitySoundEngine.SetSwitch(sw.group, sw.value, emitter);
                }
            }

            AkUnitySoundEngine.PostEvent(card.playSfxEvent, emitter);
        }
    }
}

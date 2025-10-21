using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    /// <summary>
    /// Zentrale Utilities für Venoms (Poison/Burn/Bleed/Stun).
    /// - Nutzt power_u (d.Power) als Anzahl der nächsten Angriffe (Charges).
    /// - Kapselt "Apply Next Hit ..." für DoTs/Status.
    /// - Merkt sich die zuletzt gespielte Venom-KARTE (eine Art), damit "Reapply Venom" sie erneut arm'en kann.
    /// </summary>
    public static class VenomUtility
    {
        // ------------------------------------------------------------
        // Last-Venom memory (für "Reapply Venom") – exakt wie gewünscht
        // ------------------------------------------------------------
        #region Last-Venom memory (for "Reapply Venom")
        private static readonly Dictionary<int, (string effectName, int tick, int duration, gameplayRef tickRef)> _lastVenom
            = new Dictionary<int, (string, int, int, gameplayRef)>();

        /// <summary>Merkt die zuletzt gespielte Venom-Karte (eine Effektart).</summary>
        public static void SetLastVenom(EntityScript user, string effectName, int tick, int duration, gameplayRef tickRef)
        {
            if (user == null) return;
            _lastVenom[user.GetInstanceID()] = (effectName, Mathf.Max(1, tick), Mathf.Max(1, duration), tickRef);
        }

        /// <summary>Liest die zuletzt gespielte Venom-Karte aus (falls vorhanden).</summary>
        public static bool TryGetLastVenom(
            EntityScript user,
            out (string effectName, int tick, int duration, gameplayRef tickRef) v)
        {
            if (user != null)
            {
                var id = user.GetInstanceID();
                if (_lastVenom.TryGetValue(id, out v))
                    return true;
            }
            v = default;
            return false;
        }
        #endregion

        // ------------------------------------------------------------
        // Öffentliche API: Karten-Wrapper (nutzen power_u als Charges)
        // ------------------------------------------------------------

        /// <summary>Für die nächsten (charges) Angriffe: Burn anwenden.</summary>
        public static void ArmBurnHits(EntityScript user, int tick, int duration, int charges)
        {
            ArmNextHitDotWithCharges(user, tick, duration, "Burn", gameplayRef.onBurn, charges);
            SetLastVenom(user, "Burn", tick, duration, gameplayRef.onBurn);
        }

        /// <summary>Für die nächsten (charges) Angriffe: Poison anwenden.</summary>
        public static void ArmPoisonHits(EntityScript user, int tick, int duration, int charges)
        {
            ArmNextHitDotWithCharges(user, tick, duration, "Poison", gameplayRef.onPoison, charges);
            SetLastVenom(user, "Poison", tick, duration, gameplayRef.onPoison);
        }

        /// <summary>Für die nächsten (charges) Angriffe: Bleed anwenden.</summary>
        public static void ArmBleedHits(EntityScript user, int tick, int duration, int charges)
        {
            ArmNextHitDotWithCharges(user, tick, duration, "Bleed", gameplayRef.onBleed, charges);
            SetLastVenom(user, "Bleed", tick, duration, gameplayRef.onBleed);
        }

        /// <summary>Für die nächsten (charges) Angriffe: Stun anwenden (Status, kein DoT).</summary>
        public static void ArmStunHits(EntityScript user, int duration, int charges)
        {
            // Falls du keinen CombatUtility-Status-Helper hast, kann ich dir hier den Inline-Arm-Code einbauen.
            CombatUtility.ApplyNextHitStatusWithCharges(user, duration, "Stun", gameplayRef.onStunned, charges);
            SetLastVenom(user, "Stun", 0, duration, gameplayRef.onStunned);
        }

        /// <summary>
        /// Reapply Venom: setzt einfach die zuletzt gespielte Venom-Karte erneut ein
        /// (armt sie erneut für 'charges' nächste Angriffe).
        /// </summary>
        public static void ReapplyLastVenom(EntityScript user, int charges)
        {
            if (!TryGetLastVenom(user, out var v)) return;

            if (v.effectName == "Stun" || v.tickRef == gameplayRef.onStunned)
            {
                ArmStunHits(user, v.duration, charges);
            }
            else
            {
                ArmNextHitDotWithCharges(user, v.tick, v.duration, v.effectName, v.tickRef, charges);
                // lastVenom bleibt gleich (gleiche Art), daher kein erneutes SetLastVenom nötig
            }
        }


        /// Arm't die NÄCHSTEN 'charges' Treffer mit einem DoT (Poison/Burn/Bleed).

        public static void ArmNextHitDotWithCharges(
            EntityScript user, int tick, int duration, string effectName, gameplayRef tickRef, int charges)
        {
            if (user == null || charges <= 0) return;
            tick = Mathf.Max(1, tick);
            duration = Mathf.Max(1, duration);
            for (int i = 0; i < charges; i++)
                CombatUtility.ApplyNextHitDot(user, tick, duration, effectName, tickRef);
        }

        /// Arm't die NÄCHSTEN 'charges' Treffer mit einem Status (z. B. Stun).
        public static void ArmNextHitStatusWithCharges(
            EntityScript user, int duration, string effectName, gameplayRef statusRef, int charges)
        {
            if (user == null || charges <= 0) return;
            duration = Mathf.Max(1, duration);
            CombatUtility.ApplyNextHitStatusWithCharges(user, duration, effectName, statusRef, charges);
        }

        public static void ArmBurnFromCard(EntityScript user, CardData d)
        {
            int charges = (d.Power > 0 ? d.Power : 3);
            ArmBurnHits(user, d.Damage, d.Duration, charges);
        }

        public static void ArmPoisonFromCard(EntityScript user, CardData d)
        {
            int charges = (d.Power > 0 ? d.Power : 3);
            ArmPoisonHits(user, d.Damage, d.Duration, charges);
        }

        public static void ArmBleedFromCard(EntityScript user, CardData d)
        {
            int charges = (d.Power > 0 ? d.Power : 3);
            ArmBleedHits(user, d.Damage, d.Duration, charges);
        }

        public static void ArmStunFromCard(EntityScript user, CardData d)
        {
            int charges = (d.Power > 0 ? d.Power : 3);
            ArmStunHits(user, d.Duration, charges);
        }

        public static void ReapplyFromCard(EntityScript user, CardData d)
        {
            int charges = (d.Power > 0 ? d.Power : 3);
            ReapplyLastVenom(user, charges);
        }
    }
}

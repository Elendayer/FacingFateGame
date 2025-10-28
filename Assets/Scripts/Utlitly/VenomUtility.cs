using System.Collections.Generic;
using UnityEngine;

/*
namespace Utility
{

   public static class VenomUtility
   {
       #region Last-Venom memory (for "Reapply Venom")
       private static readonly Dictionary<int, (string effectName, int tick, int duration, GameplayRef tickRef)> _lastVenom
           = new Dictionary<int, (string, int, int, GameplayRef)>();

       /// <summary>Merkt die zuletzt gespielte Venom-Karte (eine Effektart).</summary>
       public static void SetLastVenom(EntityScript user, string effectName, int tick, int duration, GameplayRef tickRef)
       {
           if (user == null) return;
           _lastVenom[user.GetInstanceID()] = (effectName, Mathf.Max(1, tick), Mathf.Max(1, duration), tickRef);
       }

       /// <summary>Liest die zuletzt gespielte Venom-Karte aus (falls vorhanden).</summary>
       public static bool TryGetLastVenom(
           EntityScript user,
           out (string effectName, int tick, int duration, GameplayRef tickRef) v)
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

       // ---------------------
       // “Arm”-Hilfsfunktionen
       // ---------------------

       //*
       public static void ArmBurnHits(EntityScript user, int tick, int duration, int charges)
       {
           ArmNextHitDotWithCharges(user, tick, duration, "Burn", GameplayRef.onBurn, charges);
           SetLastVenom(user, "Burn", tick, duration, GameplayRef.onBurn);
       }

       /// <summary>Für die nächsten (charges) Angriffe: Poison anwenden.</summary>
       public static void ArmPoisonHits(EntityScript user, int tick, int duration, int charges)
       {
           ArmNextHitDotWithCharges(user, tick, duration, "Poison", GameplayRef.onPoison, charges);
           SetLastVenom(user, "Poison", tick, duration, GameplayRef.onPoison);
       }

       /// <summary>Für die nächsten (charges) Angriffe: Bleed anwenden.</summary>
       public static void ArmBleedHits(EntityScript user, int tick, int duration, int charges)
       {
           ArmNextHitDotWithCharges(user, tick, duration, "Bleed", GameplayRef.onBleed, charges);
           SetLastVenom(user, "Bleed", tick, duration, GameplayRef.onBleed);
       }

       /// <summary>Für die nächsten (charges) Angriffe: Stun anwenden (Status, kein DoT).</summary>
       public static void ArmStunHits(EntityScript user, int duration, int charges)
       {
           // Achtung: Event-Name ist onStunned (nicht onStun).
           CombatUtility.ApplyNextHitStatusWithCharges(user, duration, "Stun", GameplayRef.onStunned, charges);
           SetLastVenom(user, "Stun", 0, duration, GameplayRef.onStunned);
       }


       /// <summary>
       /// Reapply Venom: setzt die zuletzt gespielte Venom-Karte erneut ein
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
               // lastVenom bleibt gleich (gleiche Art)
           }
       }

       /// <summary>
       /// Armt ‘charges’ nächste Treffer mit einem DoT. Die eigentliche Anlage passiert beim Treffer
       /// (siehe TryConsumeAndApplyOnHit – Hook in CombatUtility.ApplyDamage).
       /// </summary>
       public static void ArmNextHitDotWithCharges(
           EntityScript user, int tick, int duration, string effectName, gameplayRef tickRef, int charges)
       {
           if (user == null || charges <= 0) return;
           tick = Mathf.Max(1, tick);
           duration = Mathf.Max(1, duration);

           // Wir nutzen die zentrale “Next-Hit”-Pipeline in CombatUtility, damit alle Karten an EINEM Ort landen.
           for (int i = 0; i < charges; i++)
               CombatUtility.ApplyNextHitDot(user, tick, duration, effectName, tickRef);
       }

       /// <summary>Convenience: Status (kein DoT) für die nächsten ‘charges’ Angriffe vormerken.</summary>
       public static void ArmNextHitStatusWithCharges(
           EntityScript user, int duration, string effectName, gameplayRef statusRef, int charges)
       {
           if (user == null || charges <= 0) return;
           duration = Mathf.Max(1, duration);
           CombatUtility.ApplyNextHitStatusWithCharges(user, duration, effectName, statusRef, charges);
       }

       // ------------------------
       // Karten → Helper-Mapping
       // ------------------------

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


       public static void TryConsumeAndApplyOnHit(EntityScript user, EntityScript target)
       {
           //CombatUtility.TryConsumeAndApplyOnHit(user, target);
       }
   }
}

*/
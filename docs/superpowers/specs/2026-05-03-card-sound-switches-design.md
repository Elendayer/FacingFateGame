# Card Sound Switches — Design Spec
**Date:** 2026-05-03  
**Status:** Approved

## Goal
Card effects play different Wwise sounds based on two discrete dimensions: elemental identity and mechanical type. System is fully optional — missing config = silent, no errors.

## Enums (CardData.cs)

```csharp
public enum CardSoundIdentity
{
    None, Physical, Fire, Ice, Air, Earth,
    Shadow, Poison, Light, Blood, Arcane, Soul, Divine, Occult
}

public enum CardSoundDamageType
{
    None, Melee, Ranged, Magic, Healing, Buff, Debuff, Summon
}
```

Independent of `CardIdentity`. Designer sets one value per dimension per card.

## CardData Fields (under existing [Header("Audio")])

```csharp
public string              playSfxEvent;
public CardSoundIdentity   soundIdentity   = CardSoundIdentity.None;
public CardSoundDamageType soundDamageType = CardSoundDamageType.None;
```

Both fields added to `Clone()`.

## CardSoundHelper (new — Assets/Scripts/Audio/CardSoundHelper.cs)

```csharp
public static void PlayCardEffect(CardData card, GameObject emitter)
```

- `card == null` or `playSfxEvent` empty → return silently
- `soundIdentity != None` → `AkUnitySoundEngine.SetSwitch("CardSoundIdentity", ..., emitter)`
- `soundDamageType != None` → `AkUnitySoundEngine.SetSwitch("CardSoundDamageType", ..., emitter)`
- `AkUnitySoundEngine.PostEvent(card.playSfxEvent, emitter)`

**Emitter:** `refData.UserEntity?.gameObject` (the caster) — each entity owns its own
Wwise switch state, preventing contamination when multiple entities act.
Fallback: `CombatAudioController.gameObject` if UserEntity is null.

## CombatAudioController — onCardPlayed case

Replace:
```csharp
AkUnitySoundEngine.PostEvent(refData.CardData.playSfxEvent, gameObject);
```
With:
```csharp
CardSoundHelper.PlayCardEffect(refData.CardData, refData.UserEntity?.gameObject ?? gameObject);
```

## Wwise Project (manual — not in code)

Two Switch Groups:
- `"CardSoundIdentity"` — switches matching enum values (Physical, Fire, Ice, ...)
- `"CardSoundDamageType"` — switches matching enum values (Melee, Ranged, Magic, ...)

Sound designer creates Switch Containers under a single `"Play_CardEffect"` event
(or multiple events per use case). Cards reference these event name(s) via `playSfxEvent`.

## Null-Safety Rules

| Condition | Behavior |
|---|---|
| `playSfxEvent` empty | Silent, no log |
| Both enums `None` | PostEvent fires without setting any switch |
| One enum `None` | Only the other switch is set |
| `UserEntity` null | Falls back to controller GO as emitter |

## Example Card

```csharp
// SpearmanCards.cs
playSfxEvent    = "Play_CardEffect",
soundIdentity   = CardSoundIdentity.Physical,
soundDamageType = CardSoundDamageType.Melee,
```

## Files Changed

| File | Change |
|---|---|
| `CardData.cs` | +2 enum fields, +2 Clone() entries, +2 enum declarations |
| `Audio/CardSoundHelper.cs` | NEW |
| `Audio/CombatAudioController.cs` | `onCardPlayed` case → uses CardSoundHelper |

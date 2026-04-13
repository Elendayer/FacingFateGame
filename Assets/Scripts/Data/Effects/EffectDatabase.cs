using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace facingfate
{
	public static class EffectDatabase
	{
		private static Dictionary<string, EntityModifier> effectLookup = new Dictionary<string, EntityModifier>();

		public static void RegisterEffect(EntityModifier Modifier)
		{
			if (!effectLookup.ContainsKey(Modifier.ModifierName))
			{
				effectLookup[Modifier.ModifierName] = Modifier;
				//Debug.Log($"Registered card: {card.cardName} (ID: {card.cardID})");
			}
			else
			{
				Debug.LogWarning($"Duplicate card ID detected: {Modifier.ModifierName}");
			}
		}
		public static EntityModifier GetEffectByName(
			string name,
			CloneMode mode,
			CardData data,
			ThroughputSource source,
			EntityScript owner)
		{
			if (!effectLookup.TryGetValue(name, out var blueprint) || blueprint == null)
				return null;

			switch (mode)
			{
				case CloneMode.Defaults:
					return blueprint.CloneDefaults(
						data,
						source,
						owner);
				case CloneMode.OverrideFromData:
					return blueprint.CloneOverrideFromData(
						data,
						source,
						owner);
			}
			return null;
		}

		public static List<EntityModifier> GetAllEffects()
		{
			return new List<EntityModifier>(effectLookup.Values);
		}

        public static bool TryGetUIInfo(string modifierName, out string description, out Sprite icon)
        {
            description = "";
            icon = null;

            if (effectLookup.TryGetValue(modifierName, out var modifier))
            {
                description = modifier.Description;

                // Lazy load: erst jetzt laden falls noch nicht gecacht
                if (modifier.Icon == null)
                    modifier.Icon = LoadIcon(modifierName);

                icon = modifier.Icon;
                return true;
            }
            return false;
        }

        private static Sprite LoadIcon(string name)
        {
            return Resources.Load<Sprite>($"StatusIcons/{name}");
        }

        public static void RegisterAll()
		{
			RegisterEffect(new EntityModifier
				(
				modifierName: "Bleed",
                description: "Deals damage at the start of turn. Stacks increase damage dealt.",
icon: null,
                owner: null,
				duration: 3,
				toTriggerRefs: new() { GameplayRef.onBleed },
				onRef_Trigger: new RelevantTriggerCheck
				{
					OnTriggerReference = new() { GameplayRef.onTurnStart },
					CheckType = CheckEntityType.User,
					CheckEntity = null,
				},
				onRef_Action: (target, cd, value) =>
				{
					CombatUtility.ApplyEffectDamage(value, target, GameplayRef.onBleed, new VFXData ("BleedEffect",true));
				}));

			RegisterEffect(new EntityModifier
				(
				modifierName: "Poison",
                description: "Deals damage at the end of turn. Stacks increase damage dealt.",
icon: null,
                owner: null,
				duration: 2,
				toTriggerRefs: new() { GameplayRef.onPoison },
				onRef_Trigger: new RelevantTriggerCheck
				{
					OnTriggerReference = new() { GameplayRef.onTurnStart },
					CheckType = CheckEntityType.User,
					CheckEntity = null,
				},
				onRef_Action: (target, cd, value) =>
				{
					CombatUtility.ApplyEffectDamage(value, target, GameplayRef.onPoison, new VFXData("PoisonEffect", true));
                }));

			RegisterEffect(new EntityModifier
				(
				modifierName: "Burn",
                description: "Deals damage at the start of turn. Does stack but and can be refreshed to prolong the duration.",
icon: null,
                owner: null,
				duration: 2,
				toTriggerRefs: new() { GameplayRef.onBurn },
				onRef_Trigger: new RelevantTriggerCheck
				{
					OnTriggerReference = new() { GameplayRef.onTurnStart },
					CheckType = CheckEntityType.User,
					CheckEntity = null,
				},
				onRef_Action: (target, cd, value) =>
				{
					CombatUtility.ApplyEffectDamage(value, target, GameplayRef.onBurn, new VFXData("BurnEffect", true));
				}));

			RegisterEffect(new EntityModifier(
				modifierName: "Stun",
                description: "The afflicted entity skips their next turn and cannot act.",
icon: null,
                owner: null,
				toTriggerRefs: new() { GameplayRef.onStunned },
				duration: 1,
				onRef_Trigger: new RelevantTriggerCheck
				{
					OnTriggerReference = new() { GameplayRef.onTurnStart },
					CheckType = CheckEntityType.Target,
					CheckEntity = null,
				},
				onApply_Action: (target, cd, value) =>
				{
					target.entityStats.IsStunned = true;

					GameEvents.TriggerRefEvent(new ToSendTriggerReference
					{
						OnTriggerReference = new() { GameplayRef.onStunned },
						AffectedEntities = new() { },
						UserEntity = null
					});
				}));

			RegisterEffect(new EntityModifier(
				modifierName: "Taunted",
                description: "Forces the afflicted entity to target the taunting unit.",
icon: null,
                owner: null,
				toTriggerRefs: new() { GameplayRef.onTaunted },
						duration: 1,
						onRemove_Trigger: new RelevantTriggerCheck
						{
							OnTriggerReference = new() { },
							CheckType = CheckEntityType.User,
							CheckEntity = null,
						},
						actionTargetType: EntityModifier.ActionTargetType.User,
						onApply_Action: (target, cd, value) =>
						{
							target.entityStats.tauntTarget = target;
						},
						onRemove_Action: (target, cd, value) =>
						{
							target.entityStats.tauntTarget = null;
						}));
		}
	}
}
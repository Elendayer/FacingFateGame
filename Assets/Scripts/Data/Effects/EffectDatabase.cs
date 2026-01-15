using System.Collections.Generic;
using UnityEngine;
using Utility;

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
	public static EntityModifier GetEffectByName(string name, CardData d, ThroughputSource source, EntityScript owner)
	{
		if (!effectLookup.TryGetValue(name, out var blueprint) || blueprint == null)
			return null;

		EntityModifier em = blueprint.Clone(d, source, owner);
		return em;
	}

	public static List<EntityModifier> GetAllCards()
	{
		return new List<EntityModifier>(effectLookup.Values);
	}

	public static void RegisterAll()
	{
		RegisterEffect(new EntityModifier
			(
			modifierName: "Bleed",
			owner: null,
            onRef_Trigger: new RelevantTriggerCheck
			{
				OnTriggerReference = new() { GameplayRef.onTurnStart },
				CheckType = CheckEntityType.User,
				CheckEntity = null,
            },
			onRef_Action: (target, cd, value) =>
			{
				CombatUtility.ApplyEffectDamage(value, target, GameplayRef.onBleed);
			}));
	}
}
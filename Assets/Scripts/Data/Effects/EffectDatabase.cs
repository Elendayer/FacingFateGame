using System.Collections.Generic;
using UnityEngine;
using Utility;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UIElements.UxmlAttributeDescription;

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
			onRef_Trigger: new TriggerRef
			{
				OnTriggerReference = new() { GameplayRef.onTurnStart },
				AffectedEntities = new() { },
				UserEntity = null,
				CardData = null,
				Throughput = 0
			},
			onRef_Action: (data, target) =>
			{
				CombatUtility.ApplyEffectDamage(data.Value, target, GameplayRef.onBleed);
			}));
	}
}
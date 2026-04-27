using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace facingfate
{
    [System.Serializable]
    public class EntityStats
    {
        public EntityScript Owner;
        private bool deathProcessed = false;
        private bool isInitialized = false;

        [Header("Base Stats")]
        [SerializeField]
        public float CurrentHealth;

        public float MaxHealth => MaxHealth_Flat.Value() * (1f + (MaxHealth_Increase.Value() / 100f)) * GetMultiplierProduct(MaxHealth_Multiplier);
        public Stat MaxHealth_Flat = new();
        public Stat MaxHealth_Increase = new();
        public Stat MaxHealth_Multiplier = new();

        public float CurrentStamina;
        public float MaxStamina => MaxStamina_Flat.Value() * (1f + (MaxStamina_Increase.Value() / 100f)) * GetMultiplierProduct(MaxStamina_Multiplier);
        public Stat MaxStamina_Flat = new();
        public Stat MaxStamina_Increase = new();
        public Stat MaxStamina_Multiplier = new();


        [Header("Defense")]
        public float CurrentBlock = 0;
        public Stat BlockGain_Flat = new();
        public Stat BlockGain_Increase = new();
        public Stat BlockGain_Multiplier = new();

        public float CurrentArmour => Armour_Flat.Value() * (1f + (Armour_Increase.Value() / 100f)) * GetMultiplierProduct(Armour_Multiplier);

        public Stat Armour_Flat = new();
        public Stat Armour_Increase = new();
        public Stat Armour_Multiplier = new();

        [Header("Attributes")]

        public float CurrentStrength => Strength_Flat.Value() * (1f + (Strength_Increase.Value() / 100f)) * GetMultiplierProduct(Strength_Multiplier);
        public Stat Strength_Flat = new();
        public Stat Strength_Increase = new();
        public Stat Strength_Multiplier = new();

        public float CurrentDexterity => Dexterity_Flat.Value() * (1f + (Dexterity_Increase.Value() / 100f)) * GetMultiplierProduct(Dexterity_Multiplier);
        public Stat Dexterity_Flat = new();
        public Stat Dexterity_Increase = new();
        public Stat Dexterity_Multiplier = new();

        public float CurrentWisdom => Wisdom_Flat.Value() * (1f + (Wisdom_Increase.Value() / 100f)) * GetMultiplierProduct(Wisdom_Multiplier);
        public Stat Wisdom_Flat = new();
        public Stat Wisdom_Increase = new();
        public Stat Wisdom_Multiplier = new();

        public float CurrentIntelligence => Intelligence_Flat.Value() * (1f + (Intelligence_Increase.Value() / 100f)) * GetMultiplierProduct(Intelligence_Multiplier);
        public Stat Intelligence_Flat = new();
        public Stat Intelligence_Increase = new();
        public Stat Intelligence_Multiplier = new();

        public float CurrentEndurance => Endurance_Flat.Value() * (1f + (Endurance_Increase.Value() / 100f)) * GetMultiplierProduct(Endurance_Multiplier);
        public Stat Endurance_Flat = new();
        public Stat Endurance_Increase = new();
        public Stat Endurance_Multiplier = new();

        public float CurrentTenacity => Tenacity_Flat.Value() * (1f + (Tenacity_Increase.Value() / 100f)) * GetMultiplierProduct(Tenacity_Multiplier);
        public Stat Tenacity_Flat = new();
        public Stat Tenacity_Increase = new();
        public Stat Tenacity_Multiplier = new();

        [Header("Other Stats")]
        public Stat MovementCostModifier_Flat = new();
        public Stat MovementCostModifier_Increase = new();
        public Stat MovementCostModifier_Multiplier = new();

        [Header("Combat Modifiers")]
        public Stat DamageTakenModifier_Flat = new();
        public Stat DamageTakenModifier_Increase = new();
        public Stat DamageTakenModifier_Multiplier = new();

        public Stat HealingTakenModifier_Flat = new();
        public Stat HealingTakenModifier_Increase = new();
        public Stat HealingTakenModifier_Multiplier = new();

        public Stat DamageOutModifier_Flat = new();
        public Stat DamageOutModifier_Increase = new();
        public Stat DamageOutModifier_Multiplier = new();

        public Stat HealingOutModifier_Flat = new();
        public Stat HealingOutModifier_Increase = new();
        public Stat HealingOutModifier_Multiplier = new();

        public Stat CardCostModifier_Flat = new();
        public Stat CardCostModifier_Increase = new();
        public Stat CardCostModifier_Multiplier = new();

        public Stat PowerModifier_Flat = new();
        public Stat PowerModifier_Increase = new();
        public Stat PowerModifier_Multiplier = new();

        public Stat DurationModifier_Flat = new();
        public Stat DurationModifier_Increase = new();
        public Stat DurationModifier_Multiplier = new();

        public Stat AdditonalRepeatsModifier = new();

        // Percent based effects.

        public Stat IgnoreArmour = new();

        public Stat IgnoreBlock = new();

        public Stat Lifesteal = new();

        // Effect modifiers - flat increases to the base value of an effect, % increases to the base value of an effect, and % multipliers to the entire effect after all other calculations.

        public Stat RangeModifier_Flat = new();
        public Stat RangeModifier_Increase = new();
        public Stat RangeModifier_Multiplier = new();

        public Stat AreaModifier_Flat = new();
        public Stat AreaModifier_Increase = new();
        public Stat AreaModifier_Multiplier = new();

        public Stat RadiusModifier_Flat = new();
        public Stat RadiusModifier_Increase = new();
        public Stat RadiusModifier_Multiplier = new();

        public Stat AdditonalMaxTargets = new();

        [Header("StatusConditions")]
        public bool IsStunned = false;
        public bool IsRooted = false;
        public EntityScript tauntTarget;

        public void StartUp(EntityScript entityScript)
        {
            // Set the owner of the stats to the entity script
            Owner = entityScript;

            // Base attribute values - Flat
            NonPlayerScript npcScript = Owner as NonPlayerScript;
            if (npcScript != null)
            {
                Strength_Flat.AddModifier(new StatModifier("BaseValue", Strength_Flat, value: npcScript.npcData.baseStrength));
                Dexterity_Flat.AddModifier(new StatModifier("BaseValue", Dexterity_Flat, value: npcScript.npcData.baseDexterity));
                Wisdom_Flat.AddModifier(new StatModifier("BaseValue", Wisdom_Flat, value: npcScript.npcData.baseWisdom));
                Intelligence_Flat.AddModifier(new StatModifier("BaseValue", Intelligence_Flat, value: npcScript.npcData.baseIntelligence));
                Endurance_Flat.AddModifier(new StatModifier("BaseValue", Endurance_Flat, value: npcScript.npcData.baseEndurance));
                Tenacity_Flat.AddModifier(new StatModifier("BaseValue", Tenacity_Flat, value: npcScript.npcData.baseTenacity));

                MaxHealth_Flat.AddModifier(new StatModifier("BaseValue", MaxHealth_Flat, value: () => CurrentTenacity * 50f));
                MaxStamina_Flat.AddModifier(new StatModifier("BaseValue", MaxStamina_Flat, value: () => CurrentEndurance * 5f));
            }
            else
            {
                Strength_Flat.AddModifier(new StatModifier("BaseValue", Strength_Flat, value: 10f));
                Dexterity_Flat.AddModifier(new StatModifier("BaseValue", Dexterity_Flat, value: 10f));
                Wisdom_Flat.AddModifier(new StatModifier("BaseValue", Wisdom_Flat, value: 10f));
                Intelligence_Flat.AddModifier(new StatModifier("BaseValue", Intelligence_Flat, value: 10f));
                Endurance_Flat.AddModifier(new StatModifier("BaseValue", Endurance_Flat, value: 10f));
                Tenacity_Flat.AddModifier(new StatModifier("BaseValue", Tenacity_Flat, value: 10f));

                MaxHealth_Flat.AddModifier(new StatModifier("BaseValue", MaxHealth_Flat, value: () => CurrentTenacity * 50f));
                MaxStamina_Flat.AddModifier(new StatModifier("BaseValue", MaxStamina_Flat, value: () => CurrentEndurance * 5f));
            }


            CurrentHealth = GetMaxHealthValue();
            CurrentStamina = GetMaxStaminaValue();
            isInitialized = true;

            // Attribute-based damage bonuses using ConditionalModifierInfo - simple string-based condition lookup
            DamageOutModifier_Increase.AddModifier(new StatModifier("Strength", DamageOutModifier_Increase, value: () => CurrentStrength, condition: "Melee"));
            DamageOutModifier_Increase.AddModifier(new StatModifier("Dexterity", DamageOutModifier_Increase, value: () => CurrentDexterity, condition: "Ranged"));
            DamageOutModifier_Increase.AddModifier(new StatModifier("Intelligence", DamageOutModifier_Increase, value: () => CurrentIntelligence, condition: "Spell"));

            // Initial tick to set all stats correctly
            ActionQueueUtility.EnqueueAction(() =>
            {
                TickAllStats();
            });
        }

        private float GetMaxHealthValue()
        {
            float flat = MaxHealth_Flat.Value();
            float increase = 1f + (MaxHealth_Increase.Value() / 100f);
            float multipliers = GetMultiplierProduct(MaxHealth_Multiplier);
            return flat * increase * multipliers;
        }

        private float GetMaxStaminaValue()
        {
            float flat = MaxStamina_Flat.Value();
            float increase = 1f + (MaxStamina_Increase.Value() / 100f);
            float multipliers = GetMultiplierProduct(MaxStamina_Multiplier);
            return flat * increase * multipliers;
        }

        private float GetMultiplierProduct(Stat multiplierStat, EntityScript entityScript = null, CardData cardData = null)
        {
            float product = 1f;
            var multipliers = multiplierStat.GetAllMultiplierValues(entityScript, cardData);
            foreach (var mult in multipliers)
            {
                product *= (mult / 100f);
            }
            return product;
        }

        public float ApplyStatModifiers(float baseValue, Stat flatStat, Stat increaseStat, Stat multiplierStat, EntityScript entityScript = null, CardData cardData = null)
        {
            float flat = flatStat.Value(entityScript, cardData);
            float increase = increaseStat.Value(entityScript, cardData);
            float multipliers = GetMultiplierProduct(multiplierStat, entityScript, cardData);

            return (baseValue + flat) * (1f + (increase / 100f)) * multipliers;
        }

        public void TickAllStats()
        {
            var statFields = typeof(EntityStats).GetFields().Where(f => f.FieldType == typeof(Stat));

            foreach (var field in statFields)
            {
                var stat = (Stat)field.GetValue(this);
                stat.owner = Owner;
                stat?.Tick();
            }
        }

        public void UpdateStats()
        {
            // Update all stat modifiers
            var statFields = typeof(EntityStats).GetFields().Where(f => f.FieldType == typeof(Stat));

            foreach (var field in statFields)
            {
                var stat = (Stat)field.GetValue(this);
                stat?.UpdateStat();
            }

            // Refresh UI after stats are updated
            if (CombatUIController.Instance != null)
            {
                CombatUIController.Instance.RefreshAll();
            }

            Debug.Log($"Checking death for {Owner.name}", Owner);
            // Handle death condition - only process once per entity
            if (CurrentHealth <= 0 && !deathProcessed && Owner != null && isInitialized)
            {
                Debug.Log($"Death for {Owner.name}", Owner);

                deathProcessed = true;
                HandleEntityDeath();
            }
        }

        private void HandleEntityDeath()
        {
            ActionQueueUtility.EnqueueAction(() =>
            {
                try
                {
                    if (Owner == null)
                        return;

                    // 1. Mark entity as dead to prevent further interactions
                    Owner.isDead = true;

                    // 2. Clear all queued actions associated with this entity
                    ActionQueueUtility.ClearActionsBySource(Owner);

                    // 3. Remove turn safely
                    if (TurnManager.Instance != null)
                    {
                        TurnManager.Instance.RemoveTurn(Owner);
                    }

                    // 4. Remove all modifiers
                    Owner.RemoveAllModifiers();
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error during entity death handling for {Owner?.name}: {ex.Message}", Owner);
                }
            });

            // 5. Trigger death event - this will notify EncounterManager to check win/lose
            // This must happen AFTER marking as dead but BEFORE removing the entity so that triggers can still reference it
            ActionQueueUtility.EnqueueAction(() =>
            {
                try
                {
                    if (Owner != null)
                    {
                        GameEvents.TriggerRefEvent(new ToSendTriggerReference(new() { GameplayRef.onDeath }, Owner, new() { Owner }));
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error triggering death event for {Owner?.name}: {ex.Message}", Owner);
                }
            }, 0.2f);

            // 6. Create corpse and remove entity
            ActionQueueUtility.EnqueueAction(() =>
            {
                try
                {
                    if (Owner == null)
                        return;

                    GameObject corspe = GameObject.Instantiate(AssetManager.Instance.CorspePrefab, Owner.transform.position, quaternion.identity);
                    corspe.transform.rotation = new quaternion(90, 0, 0, 0);
                    corspe.transform.localScale = Owner.EntityModel.transform.localScale;
                    MeshRenderer mr = corspe.GetComponent<MeshRenderer>();
                    MeshFilter mf = corspe.GetComponent<MeshFilter>();

                    mf.mesh = Owner.EntityModel.mesh;
                    mr.materials = Owner.EntityRenderer.materials;

                    // Disable the entity (don't destroy immediately to avoid serialization issues)
                    Owner.gameObject.SetActive(false);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error creating corpse for {Owner?.name}: {ex.Message}", Owner);
                }
            }, 
            0.3f);       
        }
    }
}
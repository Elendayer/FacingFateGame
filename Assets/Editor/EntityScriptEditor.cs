using ES3Editor;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace facingfate
{
    [CustomEditor(typeof(EntityScript), true)] // ✅ Works for all subclasses
    public class EntityScriptEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw default inspector
            DrawDefaultInspector();

            EntityScript entity = (EntityScript)target;

            if (entity.entityStats == null)
            {
                EditorGUILayout.HelpBox("EntityStats is null. StartUp() may not have been called yet.", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("📊 Entity Stats (Live Values)", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            try
            {
                // === Base Stats ===
                DrawStat("Max Health", entity.entityStats.MaxHealth);
                DrawStat("Current Health", entity.entityStats.CurrentHealth);

                DrawStat("Max Stamina", entity.entityStats.MaxStamina);
                DrawStat("Current Stamina", entity.entityStats.CurrentStamina);
                EditorGUILayout.Space(5);

                // === Defense ===
                EditorGUILayout.LabelField("🛡️ Defense", EditorStyles.boldLabel);
                DrawStat("Armour", entity.entityStats.CurrentArmour);
                DrawStat("Block", entity.entityStats.CurrentBlock);
                EditorGUILayout.Space(5);

                // === Movement ===
                EditorGUILayout.LabelField("🏃 Movement", EditorStyles.boldLabel);
                DrawStat("Movement Cost", entity.entityStats.MovementCostModifier_Flat, entity.entityStats.MovementCostModifier_Increase, entity.entityStats.MovementCostModifier_Multiplier);

                // === Attributes ===
                EditorGUILayout.LabelField("💪 Attributes", EditorStyles.boldLabel);
                DrawStat("Strength", entity.entityStats.CurrentStrength);
                DrawStat("Dexterity", entity.entityStats.CurrentDexterity);
                DrawStat("Wisdom", entity.entityStats.CurrentWisdom);
                DrawStat("Foresight", entity.entityStats.CurrentIntelligence);
                DrawStat("Endurance", entity.entityStats.CurrentEndurance);
                DrawStat("Tenacity", entity.entityStats.CurrentTenacity);
                EditorGUILayout.Space(5);

                // === Combat Modifiers ===
                EditorGUILayout.LabelField("⚔️ Combat Modifiers", EditorStyles.boldLabel);
                DrawStat("Damage Increase", entity.entityStats.DamageOutModifier_Flat, entity.entityStats.DamageOutModifier_Increase, entity.entityStats.DamageOutModifier_Multiplier);
                DrawStat("Damage Reduction", entity.entityStats.DamageTakenModifier_Flat, entity.entityStats.DamageTakenModifier_Increase, entity.entityStats.DamageTakenModifier_Multiplier);
                DrawStat("Healing Increase", entity.entityStats.HealingOutModifier_Flat, entity.entityStats.HealingOutModifier_Increase, entity.entityStats.HealingOutModifier_Multiplier);
                DrawStat("Cost Increase", entity.entityStats.CardCostModifier_Flat, entity.entityStats.CardCostModifier_Increase, entity.entityStats.CardCostModifier_Multiplier);
                DrawStat("Power Increase", entity.entityStats.PowerModifier_Flat, entity.entityStats.PowerModifier_Increase, entity.entityStats.PowerModifier_Multiplier);
                DrawStat("Duration Increase", entity.entityStats.DurationModifier_Flat, entity.entityStats.DurationModifier_Increase, entity.entityStats.DurationModifier_Multiplier);
                DrawStat("Ignore Armour", entity.entityStats.IgnoreArmour);
                DrawStat("Ignore Block", entity.entityStats.IgnoreBlock);
                DrawStat("Lifesteal", entity.entityStats.Lifesteal);
                EditorGUILayout.Space(5);

                EditorGUILayout.LabelField(" Status Effects", EditorStyles.boldLabel);
                DrawStat("Stunned", entity.entityStats.IsStunned);
                DrawStat("Rooted", entity.entityStats.IsRooted);
                DrawStat("Taunt Target", entity.entityStats.tauntTarget ? entity.entityStats.tauntTarget.name : "not Taunted");
                EditorGUILayout.Space(6);
            }
            catch
            {
                EditorGUILayout.HelpBox("Some stats may not be initialized yet. Run StartUp() to populate them.", MessageType.Info);
            }

            EditorGUILayout.Space(10);
            DrawEntityModifiers(entity);

            if (Application.isPlaying)
                Repaint();
        }
        private void DrawStat(string label, Stat stat)
        {           
            // Column widths for clean alignment
            float colLabel = 120f;
            float colValue = 60f;

            float value = stat.Value();

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Width(colLabel));
            EditorGUILayout.LabelField(stat.Value().ToString());
            EditorGUILayout.EndHorizontal();
        }

        private void DrawStat(string label, Stat stat_Flat, Stat stat_Increase, Stat stat_Multiplier)
        {
            // Column widths for clean alignment
            float colLabel = 120f;
            float colValue = 60f;
            float colFlat = 50f;
            float colPercent = 50f;
            float colMult = 50f;

            float flat = stat_Flat.Value();
            float percent = stat_Increase.Value();
            List<float> multipliers = stat_Multiplier.GetAllMultiplierValues();


            // Main Row (first multiplier if present)
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Width(colLabel)); // empty because label already shown above
            EditorGUILayout.LabelField(flat.ToString(), GUILayout.Width(colFlat));
            EditorGUILayout.LabelField(percent.ToString(), GUILayout.Width(colPercent));

            if (multipliers.Count > 0)
            {
                string multStr = "";
                foreach (float mult in multipliers)
                {
                     multStr = mult.ToString() + ", ";
                }
                EditorGUILayout.LabelField(multStr, GUILayout.Width(colMult));
            }
            else
            {
                EditorGUILayout.LabelField("-", GUILayout.Width(colMult));
            }

            EditorGUILayout.EndHorizontal();
        }
        private void DrawStat(string label, float stat)
        {
            float colLabel = 120f;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Width(colLabel));
            EditorGUILayout.LabelField(stat.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }
        private void DrawStat(string label, bool stat)
        {
            float colLabel = 120f;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Width(colLabel));
            EditorGUILayout.LabelField(stat.ToString(), EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }
        private void DrawStat(string label, string stat)
        {
            float colLabel = 120f;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel, GUILayout.Width(colLabel));
            EditorGUILayout.LabelField(stat, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
        }

        private void DrawEntityModifiers(EntityScript entity)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Active Entity Modifiers", EditorStyles.boldLabel);

            // Use reflection to get the private 'entityModifiers' field
            FieldInfo modifiersField = typeof(EntityScript).GetField("entityModifiers", BindingFlags.NonPublic | BindingFlags.Instance);
            if (modifiersField == null)
            {
                EditorGUILayout.LabelField("Could not find entityModifiers field.");
                return;
            }

            var list = modifiersField.GetValue(entity) as IEnumerable<IEntityModifier>;
            if (list == null)
            {
                EditorGUILayout.LabelField("No active modifiers.");
                return;
            }

            int count = 0;
            foreach (IEntityModifier mod in list)
            {
                if (mod == null) continue;
                count++;

                string name = mod.ModifierName ?? "<Unnamed>";

                GameplayRef condition;
                if (mod.OnRef_Trigger.OnTriggerReference != null)
                {
                    condition = mod.OnRef_Trigger.OnTriggerReference.FirstOrDefault();
                }
                else
                {
                    condition = GameplayRef.None;
                }
                string valueStr = "";
                int duration = mod.Duration;

                // Try to read "BaseValue" or "Value" if they exist
                var baseValField = mod.GetType().GetField("BaseValue", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var baseValProp = mod.GetType().GetProperty("BaseValue", BindingFlags.Public | BindingFlags.Instance);
                var valueProp = mod.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance);

                if (baseValField != null)
                    valueStr = baseValField.GetValue(mod)?.ToString();
                else if (baseValProp != null)
                    valueStr = baseValProp.GetValue(mod)?.ToString();
                else if (valueProp != null)
                    valueStr = valueProp.GetValue(mod)?.ToString();

                EditorGUILayout.BeginHorizontal("box");
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField($"{count}. {name}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Value: {valueStr}", EditorStyles.boldLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical("Box");
                EditorGUILayout.LabelField($"Condition: {condition}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Duration: {duration}", EditorStyles.boldLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }

            if (count == 0)
                EditorGUILayout.LabelField("No active modifiers.");
        }

        public override bool RequiresConstantRepaint() => Application.isPlaying;
    }
}
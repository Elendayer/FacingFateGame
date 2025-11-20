using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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
            DrawStat("Armour", entity.entityStats.Armour);
            DrawStat("Block", entity.entityStats.Block);
            EditorGUILayout.Space(5);

            // === Attributes ===
            EditorGUILayout.LabelField("💪 Attributes", EditorStyles.boldLabel);
            DrawStat("Strength", entity.entityStats.Strength);
            DrawStat("Dexterity", entity.entityStats.Dexterity);
            DrawStat("Wisdom", entity.entityStats.Wisdom);
            DrawStat("Foresight", entity.entityStats.Foresight);
            DrawStat("Endurance", entity.entityStats.Endurance);
            DrawStat("Tenacity", entity.entityStats.Tenacity);
            EditorGUILayout.Space(5);

            // === Combat Modifiers ===
            EditorGUILayout.LabelField("⚔️ Combat Modifiers", EditorStyles.boldLabel);
            DrawStat("Damage Increase", entity.entityStats.DamageOutModifier);
            DrawStat("Damage Reduction", entity.entityStats.DamageTakenModifier);
            DrawStat("Healing Increase", entity.entityStats.HealingOutModifier);
            DrawStat("Cost Increase", entity.entityStats.CostModifier);
            DrawStat("Power Increase", entity.entityStats.PowerModifier);
            DrawStat("Duration Increase", entity.entityStats.DurationModifier);
            DrawStat("Ignore Armour", entity.entityStats.IgnoreArmour);
            DrawStat("Ignore Block", entity.entityStats.IgnoreBlock);
            DrawStat("Lifesteal", entity.entityStats.Lifesteal);
            EditorGUILayout.Space(10);    
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
        if (stat == null)
        {
            EditorGUILayout.LabelField($"{label}: null");
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(160));
        EditorGUILayout.LabelField(stat.Value.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
    }
    private void DrawStat(string label, int stat)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, GUILayout.Width(160));
        EditorGUILayout.LabelField(stat.ToString(), EditorStyles.boldLabel);
        EditorGUILayout.EndHorizontal();
    }

    private void DrawEntityModifiers(EntityScript entity)
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🧩 Active Entity Modifiers", EditorStyles.boldLabel);

        // Use reflection to get the private 'entityModifiers' field
        FieldInfo modifiersField = typeof(EntityScript).GetField("entityModifiers", BindingFlags.NonPublic | BindingFlags.Instance);
        if (modifiersField == null)
        {
            EditorGUILayout.LabelField("❌ Could not find entityModifiers field.");
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
            GameplayRef condition = mod.OnTriggerConditionRef.OnTriggerReference.FirstOrDefault();
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

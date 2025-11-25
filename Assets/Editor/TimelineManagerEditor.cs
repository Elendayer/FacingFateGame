// Editor/TimelineManagerEditor.cs
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(TimelineManager))]
public class TimelineManagerEditor : Editor
{
    private bool showTimeline = true;
    private Vector2 scrollPos;

    // Tracks which turn entries are expanded
    private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

    // Cache of solid color textures for backgrounds
    private readonly Dictionary<Color, Texture2D> colorCache = new Dictionary<Color, Texture2D>();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Timeline", EditorStyles.boldLabel);

        showTimeline = EditorGUILayout.Foldout(showTimeline, $"Timeline Entries ({TimelineManager.Timeline.Count})", true);
        if (!showTimeline) return;

        EditorGUI.indentLevel++;
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox, GUILayout.Height(600));

        int entryIndex = 0;
        foreach (var kvp in TimelineManager.Timeline)
        {
            string key = kvp.Key; // Example: "UserId_TurnIndex"
            var triggerList = kvp.Value;

            if (!foldouts.ContainsKey(key))
                foldouts[key] = false;

            // Collapsible section per turn
            GUI.backgroundColor = new Color(0.15f, 0.2f, 0.25f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            foldouts[key] = EditorGUILayout.Foldout(foldouts[key],
                $"▶ Turn {key} ({triggerList.Count} triggers)", true, EditorStyles.foldoutHeader);

            if (foldouts[key])
            {
                EditorGUI.indentLevel++;
                foreach (var triggerRef in triggerList)
                {
                    bool isUserTurn = key.EndsWith(triggerRef.UserId.ToString());

                    // Green if user’s own turn, red otherwise
                    Color bgColor = isUserTurn ? new Color(0.35f, 0.55f, 0.35f, 0.8f)
                                               : new Color(0.6f, 0.3f, 0.3f, 0.8f);

                    DrawTriggerBox(triggerRef, bgColor);
                    GUILayout.Space(4);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            GUILayout.Space(8);
            entryIndex++;
        }

        EditorGUILayout.EndScrollView();
        EditorGUI.indentLevel--;
    }

    private void DrawTriggerBox(TriggerRef triggerRef, Color bgColor)
    {
        var boxStyle = new GUIStyle()
        {
            normal = { background = GetColorTexture(bgColor) }
        };

        EditorGUILayout.BeginHorizontal(boxStyle);
        {
            // Left Column: Basic Info
            EditorGUILayout.BeginVertical(boxStyle);
            EditorGUILayout.LabelField($"User ID: {triggerRef.UserId}");
            EditorGUILayout.LabelField("Affected Entity ID", EditorStyles.boldLabel);
            if (triggerRef.AffectedEntitiesIds != null && triggerRef.AffectedEntitiesIds.Count > 0)
            {
                EditorGUI.indentLevel++;
                foreach (var id in triggerRef.AffectedEntitiesIds)
                    EditorGUILayout.LabelField($"• {id}");
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("— None —");
            }
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Card Data:", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(triggerRef.CardData != null ? triggerRef.CardData.cardName : "null");
            EditorGUILayout.EndVertical();

            // Right Column: Gameplay References
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Gameplay References:", EditorStyles.boldLabel);
            if (triggerRef.OnTriggerReference != null && triggerRef.OnTriggerReference.Count > 0)
            {
                EditorGUI.indentLevel++;
                foreach (var reference in triggerRef.OnTriggerReference)
                    EditorGUILayout.LabelField($"• {reference}");
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.LabelField("— None —");
            }
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"Throughput {triggerRef.Throughput}", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
    }

    private Texture2D GetColorTexture(Color color)
    {
        if (!colorCache.ContainsKey(color))
        {
            var tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            colorCache[color] = tex;
        }
        return colorCache[color];
    }
}

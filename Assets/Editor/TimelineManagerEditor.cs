// Editor/TimelineManagerEditor.cs
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

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

            string name =(kvp.Key.Split('_').Last()); 
            var triggerList = kvp.Value;

            if (!foldouts.ContainsKey(name))
                foldouts[name] = false;

            // Collapsible section per turn
            GUI.backgroundColor = new Color(0.15f, 0.2f, 0.25f);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;

            foldouts[name] = EditorGUILayout.Foldout(foldouts[name],
                $"▶Round:{TurnManager.Instance.CurrentRoundIndex}_{name}", true, EditorStyles.foldoutHeader);

            if (foldouts[name])
            {
                Color bgColor = Color.white;

                EditorGUI.indentLevel++;
                foreach (var triggerRef in triggerList)
                {
                    var affected = triggerRef.AffectedEntities ?? new List<EntityScript>();


                    // if no affected entities, gray
                    if (affected.Count == 0)
                    {
                        bgColor = Color.gray;
                    }
                    else if (affected.Count == 1 && affected[0] == triggerRef.UserEntity)
                    {
                        bgColor = Color.cyan;
                    }
                    // Green if all allies affected
                    else if (affected.All(e => e.entityAffiliation == triggerRef.UserEntity.entityAffiliation))
                    {
                        bgColor = Color.green;
                    }
                    // Red if all enemies affected
                    else if (affected.All(e => e.entityAffiliation != triggerRef.UserEntity.entityAffiliation))
                    {
                        bgColor = Color.red;
                    }
                    // cyan if only self affected
                    else if (affected.Count == 1 && affected[0] == triggerRef.UserEntity)
                    {
                        bgColor = Color.cyan;
                    }
                    // Yellow if mixed
                    else
                    {
                        bgColor = Color.yellow;
                    }

                    bgColor.a = 0.1f;

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

    private void DrawTriggerBox(ToSendTriggerReference triggerRef, Color bgColor)
    {
        var boxStyle = new GUIStyle()
        {
            normal = { background = GetColorTexture(bgColor) },
            padding = new RectOffset(8, 8, 8, 8)
        };

        EditorGUILayout.BeginHorizontal(boxStyle);
        try
        {
            // LEFT COLUMN
            EditorGUILayout.BeginVertical();
            try
            {
                string userIdSafe = triggerRef.UserEntity != null
                    ? triggerRef.UserEntity.name
                    : "(null UserEntity)";

                EditorGUILayout.LabelField($"User ID: {userIdSafe}");

                EditorGUILayout.LabelField("Affected Entity ID", EditorStyles.boldLabel);

                if (triggerRef.AffectedEntities != null && triggerRef.AffectedEntities.Count > 0)
                {
                    EditorGUI.indentLevel++;
                    foreach (var id in triggerRef.AffectedEntities)
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
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }

            // RIGHT COLUMN
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            try
            {
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

                EditorGUILayout.Space(4);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                try
                {
                    EditorGUILayout.LabelField($"Throughput {triggerRef.Throughput}", EditorStyles.boldLabel);
                }
                finally
                {
                    EditorGUILayout.EndVertical();
                }
            }
            finally
            {
                EditorGUILayout.EndVertical();
            }
        }
        finally
        {
            EditorGUILayout.EndHorizontal();
        }
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

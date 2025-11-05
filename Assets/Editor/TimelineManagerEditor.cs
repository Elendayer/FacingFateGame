// Editor/TimelineManagerEditor.cs
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TimelineManager))]
public class TimelineManagerEditor : Editor
{
    private bool showTimeline = true;
    private Vector2 scrollPos;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Timeline (Read-Only)", EditorStyles.boldLabel);

        showTimeline = EditorGUILayout.Foldout(showTimeline, $"TriggerRefs ({TimelineManager.Timeline.Count})", true);
        if (!showTimeline) return;

        EditorGUI.indentLevel++;
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Height(300));

        for (int i = 0; i < TimelineManager.Timeline.Count; i++)
        {
            var triggerRef = TimelineManager.Timeline[i];
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Trigger {i}", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("User ID", triggerRef.UserId.ToString());
            EditorGUILayout.LabelField("Affected Entity ID", triggerRef.AffectedEntityId.ToString());

            // Display CardData as text instead of ObjectField
            EditorGUILayout.LabelField("Card Data", triggerRef.CardData != null ? triggerRef.CardData.ToString() : "null");

            EditorGUILayout.LabelField("Gameplay References:");
            if (triggerRef.References != null && triggerRef.References.Count > 0)
            {
                EditorGUI.indentLevel++;
                for (int r = 0; r < triggerRef.References.Count; r++)
                {
                    EditorGUILayout.LabelField($"Ref {r}: {triggerRef.References[r].ToString() ?? "null"}");
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        EditorGUILayout.EndScrollView();
        EditorGUI.indentLevel--;
    }
}

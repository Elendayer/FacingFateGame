using facingfate;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class EntityVisualScript : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public TextMeshPro textMeshProUGUI;
    public GameObject UIAnchor;
    public LineRenderer lineRenderer;

    public float circleRadius = 1f;
    public int circleResolution = 50;
    public float heightOffset = 0.05f;

    public EntityScript EntityScript;

    private bool _isActiveTurn = false;

    private void Awake()
    {
        DrawCircle();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Update()
    {
        textMeshProUGUI.text = transform.parent.name;

        if (UIAnchor != null)
        {
            UIAnchor.transform.position = transform.position;
            UIAnchor.transform.rotation = Quaternion.LookRotation(Vector3.forward);
        }
    }

    public void DrawCircle()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.positionCount = circleResolution + 1;
        Vector3[] positions = new Vector3[circleResolution + 1];

        for (int i = 0; i <= circleResolution; i++)
        {
            float angle = (float)i / circleResolution * 2f * Mathf.PI;
            positions[i] = new Vector3(
                Mathf.Cos(angle) * circleRadius,
                heightOffset,
                Mathf.Sin(angle) * circleRadius
            );
        }

        lineRenderer.SetPositions(positions);
    }

    public void HighlightTurn()
    {
        _isActiveTurn = true;
        if (lineRenderer == null)
            return;

        lineRenderer.startColor = new Color(1f, 0.84f, 0f); // Gold
        lineRenderer.endColor = new Color(1f, 0.84f, 0f);
    }

    public void HighlightAffectedByCardEffect()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.startColor = new Color(1f, 0.5f, 0f); // Orange
        lineRenderer.endColor = new Color(1f, 0.5f, 0f);
    }

    public void HighlightSelection()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.startColor = new Color(0f, 1f, 0f); // Green
        lineRenderer.endColor = new Color(0f, 1f, 0f);
    }

    public void HighlightInAreaOfEffect()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.startColor = new Color(1f, 0f, 0f); // Red
        lineRenderer.endColor = new Color(1f, 0f, 0f);
    }

    public void HighlightHoverTarget()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.startColor = new Color(0f, 1f, 1f); // Cyan
        lineRenderer.endColor = new Color(0f, 1f, 1f);
    }

    public void ClearHighlightEndTurn()
    {
        if (lineRenderer == null)
            return;

        // Don't clear the circle if it's this entity's active turn — restore turn color instead
        if (_isActiveTurn)
        {
            HighlightTurn();
            return;
        }

        lineRenderer.startColor = Color.clear;
        lineRenderer.endColor = Color.clear;
    }

    /// <summary>Called by EndTurn to fully clear the circle including the turn flag.</summary>
    public void ClearTurnHighlight()
    {
        _isActiveTurn = false;
        if (lineRenderer == null) return;
        lineRenderer.startColor = Color.clear;
        lineRenderer.endColor = Color.clear;
    }

    public void ClearHighlight()
    {
        if (lineRenderer == null)
            return;

        lineRenderer.startColor = Color.clear;
        lineRenderer.endColor = Color.clear;

        if (TurnManager.Instance.CurrentTurnEntity == EntityScript)
        {
            HighlightTurn();
        }
    }
}
using UnityEngine;
using UnityEngine.UI;

namespace facingfate
{
    /// <summary>
    /// Steuert die bestehende Unity Outline Component auf CardFace
    /// um die Kartenauswahl visuell darzustellen.
    /// </summary>
    public class CardOutline : MonoBehaviour
    {
        [SerializeField] private Outline outline; // die Outline auf CardFace
        [SerializeField] private Color selectedColor = new Color(1f, 0.85f, 0.2f, 1f);
        [SerializeField] private Color normalColor = new Color(0f, 0f, 0f, 1f);
        [SerializeField] private Vector2 selectedDistance = new Vector2(3f, -3f);
        [SerializeField] private Vector2 normalDistance = new Vector2(1f, -1f);

        private void Awake()
        {
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (outline == null) return;
            outline.effectColor = selected ? selectedColor : normalColor;
            outline.effectDistance = selected ? selectedDistance : normalDistance;
        }
    }
}
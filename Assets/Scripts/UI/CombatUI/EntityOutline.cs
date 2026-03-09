using UnityEngine;

namespace facingfate
{
    public class EntityOutline : MonoBehaviour
    {
        [Header("Tint Colors")]
        [SerializeField] private Color hoverColor = new Color(1f, 1f, 0.5f, 1f); // Gelblich
        [SerializeField] private Color lockedColor = new Color(0.5f, 1f, 0.5f, 1f); // Grünlich
        [SerializeField] private Color normalColor = Color.white;

        private SpriteRenderer _sr;

        private void Awake()
        {
            // Erst am Root suchen, dann im direkten Kind "Entity" oder "Body"
            _sr = GetComponent<SpriteRenderer>();

            if (_sr == null)
            {
                // Bei Bone-Entities: ersten SpriteRenderer in direkten Children suchen
                // (nicht tief rekursiv, sonst erwischt man Bone-Renderer)
                foreach (Transform child in transform)
                {
                    _sr = child.GetComponent<SpriteRenderer>();
                    if (_sr != null) break;
                }
            }
        }

        public void SetHover(bool active)
        {
            if (_sr == null) return;
            _sr.color = active ? hoverColor : normalColor;
        }

        public void SetLocked(bool active)
        {
            if (_sr == null) return;
            _sr.color = active ? lockedColor : normalColor;
        }

        public void SetNormal()
        {
            if (_sr == null) return;
            _sr.color = normalColor;
        }
    }
}

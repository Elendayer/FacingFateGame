using UnityEngine;
using TMPro;

namespace facingfate
{

    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance { get; private set; }

        [SerializeField] private RectTransform root;
        [SerializeField] private TMP_Text headerText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Vector2 screenOffset = new Vector2(16f, -16f);

        private Canvas canvas;

        private void Awake()
        {
            Instance = this;
            canvas = GetComponentInParent<Canvas>();
            Hide();
        }

        private void LateUpdate()
        {
            if (root == null || !root.gameObject.activeSelf) return;
            FollowMouse();
        }

        public void Show(string header, string body)
        {
            if (root == null) return;

            if (headerText != null) headerText.text = header ?? "";
            if (bodyText != null) bodyText.text = body ?? "";

            root.gameObject.SetActive(true);
            FollowMouse();
        }

        public void Hide()
        {
            if (root == null) return;
            root.gameObject.SetActive(false);
        }

        private void FollowMouse()
        {
            if (canvas == null || root == null) return;

            Vector2 pos = Input.mousePosition;
            pos += screenOffset;

            root.position = pos;
        }
    }
}

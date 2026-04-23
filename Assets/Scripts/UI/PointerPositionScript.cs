using UnityEngine;

namespace facingfate
{
    /// <summary>
    /// Attach to an arrow UI element. Each frame converts a world-space Transform
    /// to screen position and moves this RectTransform to follow it.
    /// Enabled/disabled by TutorialHighlightArrow per arrow instance.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class PointerPositionScript : MonoBehaviour
    {
        private Transform _worldTarget;
        private RectTransform _rect;
        private Canvas _rootCanvas;
        private Camera _uiCamera;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();

            Canvas c = GetComponentInParent<Canvas>();
            if (c != null) _rootCanvas = c.rootCanvas;
        }

        [Tooltip("Additional screen-space offset to compensate for entity pivot vs visual center (pixels).")]
        public Vector2 targetScreenOffset;

        private ArrowDirection _direction;
        private float _arrowDistance = 60f;

        public void SetTarget(Transform target, ArrowDirection direction, float arrowDistance)
        {
            _worldTarget    = target;
            _direction      = direction;
            _arrowDistance  = arrowDistance;

            if (_rootCanvas != null)
                _uiCamera = _rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                    ? null
                    : _rootCanvas.worldCamera;
        }

        private void Update()
        {
            if (_worldTarget == null || Camera.main == null || _rootCanvas == null) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(_worldTarget.position);

            // Behind camera — hide
            if (screenPos.z < 0f)
            {
                gameObject.SetActive(false);
                return;
            }

            // Directional offset: convert canvas units → screen pixels via scaleFactor
            float pixelDist = _arrowDistance * _rootCanvas.scaleFactor;
            Vector2 offset  = GetOffset(_direction, pixelDist) + targetScreenOffset;

            if (_rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Overlay: world position == screen pixel position directly
                _rect.position = new Vector3(screenPos.x + offset.x, screenPos.y + offset.y, 0f);
            }
            else
            {
                // Screen Space Camera / World Space
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _rootCanvas.transform as RectTransform,
                    new Vector2(screenPos.x + offset.x, screenPos.y + offset.y),
                    _uiCamera,
                    out Vector2 localPoint
                );
                _rect.position = _rootCanvas.transform.TransformPoint(localPoint);
            }
        }

        private static Vector2 GetOffset(ArrowDirection dir, float dist) => dir switch
        {
            ArrowDirection.Left  => new Vector2(-dist,    0f),
            ArrowDirection.Right => new Vector2( dist,    0f),
            ArrowDirection.Up    => new Vector2(   0f,  dist),
            ArrowDirection.Down  => new Vector2(   0f, -dist),
            _                    => new Vector2(-dist,    0f),
        };
    }
}

using UnityEngine;
using UnityEngine.UI;

namespace facingfate
{
    /// <summary>
    /// Reduces scroll wheel sensitivity for dropdown lists to make selection easier.
    /// Attaches to the dropdown's ScrollRect template and reduces scroll delta.
    /// </summary>
    public class DropdownScrollSensitivityReducer : MonoBehaviour
    {
        [SerializeField] private float scrollSensitivity = 0.1f; // Reduced from default ~1.0

        private ScrollRect _scrollRect;
        private float _targetScrollPosition;
        private bool _isScrolling;

        private void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            if (_scrollRect == null)
            {
                Debug.LogWarning("DropdownScrollSensitivityReducer: ScrollRect not found on this GameObject!");
                enabled = false;
            }
        }

        private void Update()
        {
            // Only process scroll input when the dropdown is visible/active
            if (_scrollRect == null || !_scrollRect.gameObject.activeInHierarchy) return;

            // Detect scroll wheel input
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                // Apply reduced sensitivity
                _targetScrollPosition = _scrollRect.verticalNormalizedPosition + (scrollDelta * scrollSensitivity);
                _scrollRect.verticalNormalizedPosition = Mathf.Clamp01(_targetScrollPosition);
                _isScrolling = true;
            }
            else
            {
                _isScrolling = false;
            }
        }
    }
}

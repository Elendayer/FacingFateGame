using UnityEngine;
using UnityEngine.InputSystem;

namespace facingfate
{
    public class InputManager : MonoBehaviour
    {
        private static InputManager instance;

        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<InputManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("InputManager");
                        instance = go.AddComponent<InputManager>();
                    }
                }
                return instance;
            }
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        /// <summary>
        /// Returns true if the left mouse button was pressed this frame.
        /// </summary>
        public bool IsLeftMouseButtonPressed => Mouse.current.leftButton.wasPressedThisFrame;

        /// <summary>
        /// Returns true if the left mouse button was released this frame.
        /// </summary>
        public bool IsLeftMouseButtonReleased => Mouse.current.leftButton.wasReleasedThisFrame;

        /// <summary>
        /// Returns true if the left mouse button is currently held down.
        /// </summary>
        public bool IsLeftMouseButtonHeld => Mouse.current.leftButton.isPressed;

        /// <summary>
        /// Gets the current mouse position in screen space.
        /// </summary>
        public Vector2 MousePositionScreen => Mouse.current.position.ReadValue();

        /// <summary>
        /// Gets the current mouse position in world space.
        /// </summary>
        public Vector3 MousePositionWorld => Camera.main.ScreenToWorldPoint( Mouse.current.position.ReadValue());

        /// <summary>
        /// Returns true if the specified key was pressed this frame.
        /// </summary>
        public bool IsKeyPressed(Key key) => Keyboard.current[key].wasPressedThisFrame;

        /// <summary>
        /// Returns true if the specified key was released this frame.
        /// </summary>
        public bool IsKeyReleased(Key key) => Keyboard.current[key].wasReleasedThisFrame;

        /// <summary>
        /// Returns true if the specified key is currently held down.
        /// </summary>
        public bool IsKeyHeld(Key key) => Keyboard.current[key].isPressed;

        /// <summary>
        /// Gets a ray from the camera through the current mouse position.
        /// </summary>
        public Ray GetScreenRay() => Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        /// <summary>
        /// Attempts to raycast from the mouse position and returns the hit information.
        /// Returns true if a hit was detected.
        /// </summary>
        public bool TryRaycastFromMouse(out RaycastHit hit, float maxDistance = Mathf.Infinity, int layerMask = -1)
        {
            Ray ray = GetScreenRay();
            return Physics.Raycast(ray, out hit, maxDistance, layerMask);
        }

        /// <summary>
        /// Attempts to raycast from the mouse position using a specific LayerMask.
        /// Returns true if a hit was detected.
        /// </summary>
        public bool TryRaycastFromMouse(out RaycastHit hit, LayerMask layerMask)
        {
            Ray ray = GetScreenRay();
            return Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask);
        }

        /// <summary>
        /// Casts a ray from the mouse position and returns all hits.
        /// </summary>
        public RaycastHit[] RaycastAllFromMouse(float maxDistance = Mathf.Infinity, int layerMask = -1)
        {
            Ray ray = GetScreenRay();
            return Physics.RaycastAll(ray, maxDistance, layerMask);
        }
    }
}

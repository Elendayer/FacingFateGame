using UnityEngine;
using Utility;

namespace facingfate
{
    public class TileHoverOutline : MonoBehaviour
    {
        [SerializeField] private Camera hoverCamera;
        [SerializeField, Min(0.01f)] private float pollInterval = 0.05f;

        private float _timer;
        private Vector3Int _lastCell = TilemapUtilityScript.InvalidPosition;

        private void Awake()
        {
            if (hoverCamera == null) hoverCamera = Camera.main;
        }

        private void OnDisable()
        {
            // Highlight entfernen wenn Script deaktiviert wird
            ClearLastHighlight();
        }

        private void Update()
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer < pollInterval) return;
            _timer = 0f;

            Vector3 mousePos = Input.mousePosition;
            if (mousePos.x < 0 || mousePos.y < 0 ||
                mousePos.x > Screen.width || mousePos.y > Screen.height)
            {
                ClearLastHighlight();
                return;
            }

            Ray ray = hoverCamera.ScreenPointToRay(mousePos);
            Vector3Int cell = TargetingUtility.GetHoveredTile(ray);

            if (cell == TilemapUtilityScript.InvalidPosition)
            {
                ClearLastHighlight();
                return;
            }

            // Nur updaten wenn sich das Tile geändert hat
            if (cell == _lastCell) return;

            ClearLastHighlight();
            _lastCell = cell;

            // Highlight setzen (Selected = gelb/thick – kannst du anpassen)
            TilemapUtilityScript.SetTilesHighlight(
                new System.Collections.Generic.List<Vector3Int> { cell },
                TilemapUtilityScript.HighlightType.Selected
            );
        }

        private void ClearLastHighlight()
        {
            if (_lastCell == TilemapUtilityScript.InvalidPosition) return;

            TilemapUtilityScript.ResetMaphightlight(
                new System.Collections.Generic.List<Vector3Int> { _lastCell }
            );
            _lastCell = TilemapUtilityScript.InvalidPosition;
        }
    }
}

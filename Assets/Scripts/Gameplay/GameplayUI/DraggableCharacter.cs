using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class DraggableCharacter : Draggable
{
    public EntityOnMap character;
    public Tilemap baseTilemap;
    [SerializeField] private Camera worldCamera; // Kamera, die die Tilemap rendert

    protected override void Awake()
    {
        base.Awake();
        if (!worldCamera) worldCamera = Camera.main; // Fallback
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);

        var cam = worldCamera;
        if (cam == null)
        {
            Debug.LogError("Keine Weltkamera gesetzt. Weisen Sie 'worldCamera' im Inspector zu.");
            return;
        }

        // WICHTIG: Screen-Pos aus dem Event, nicht Input.mousePosition
        Vector2 screen = eventData.position;

        // Ray durch die Game-View-Position
        Ray ray = cam.ScreenPointToRay(screen);

        // Ebene der Tilemap (XY-Ebene der Tilemap-Transform)
        Plane plane = new Plane(baseTilemap.transform.forward, baseTilemap.transform.position);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 worldPos = ray.GetPoint(enter);
            Vector3Int cell = baseTilemap.WorldToCell(worldPos);
            cell.z = 0;

            // Optional: zum Gegencheck die Zellmitte loggen
            Vector3 center = baseTilemap.GetCellCenterWorld(cell);

            Debug.Log(
                $"Screen={screen} → World={worldPos} → Cell={cell} | Center={center} | Cam={cam.name}"
            );

            if (character != null && baseTilemap.HasTile(cell))
            {
                character.MoveTo(cell);
            }
        }
        else
        {
            Debug.LogWarning("Ray verfehlt die Tilemap-Ebene (stimmt die Tilemap-Ausrichtung?).");
        }
    }
}

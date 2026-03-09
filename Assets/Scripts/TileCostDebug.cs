using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileCostDebug : MonoBehaviour
{
    public Tilemap baseMap;
    public TileInfoScript costInfo;
    public Text readout; // optional

    void Update()
    {
        if (baseMap == null || costInfo == null) return;
        var cam = Camera.main; if (cam == null) return;

        var mp = Input.mousePosition;
        float z = cam.orthographic ? Mathf.Abs(cam.transform.position.z) : Mathf.Max(0.01f, cam.nearClipPlane + 0.01f);
        var world = cam.ScreenToWorldPoint(new Vector3(mp.x, mp.y, z)); world.z = 0f;

        var cell = baseMap.WorldToCell(world);
        int key = Utility.TilemapUtilityScript.PositionToKey(cell);
        if (costInfo.tileInfoDict.TryGetValue(key, out var info))
        {
            string s = $"Cell {cell.x},{cell.y} | cost={info.cost} | unwalk={info.isUnwalkable} | occ={info.isOccupied}";
            if (readout != null) readout.text = s; else Debug.Log(s);
        }
        else if (readout != null) readout.text = $"Cell {cell.x},{cell.y} | no data";
    }
}

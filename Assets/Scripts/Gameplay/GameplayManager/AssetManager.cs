using System.Collections.Generic;
using UnityEngine;
using Utility;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance { get; private set; }

    [System.Serializable]
    public struct AssetEntry
    {
        public string name;       // Key name you set in the inspector
        public GameObject prefab; // Prefab you assign
    }

    [SerializeField] private List<AssetEntry> effectAssets;
    
    private Dictionary<string, GameObject> effectAssetDict;

    [Header("Entity")]
    public GameObject entityPrefab;

    public GameObject groundEffectPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        // Build dictionary
        effectAssetDict = new Dictionary<string, GameObject>();
        foreach (var entry in effectAssets)
        {
            if (!string.IsNullOrEmpty(entry.name) && !effectAssetDict.ContainsKey(entry.name))
            {
                effectAssetDict.Add(entry.name, entry.prefab);
            }
        }
    }
    public GameObject GetEffectPrefab(string name)
    {
        if (effectAssetDict.TryGetValue(name, out GameObject prefab))
        {
            return prefab;
        }

        Debug.LogError($"Prefab with name '{name}' not found!");
        return null;
    }
    public void CreateFX(string name, Vector3Int positon)
    {
        GameObject effectObj;
        Vector3 pos = TilemapUtilityScript.BaseTilemap.CellToWorld( positon);

        effectObj = AssetManager.Instance.GetEffectPrefab(name);
        var CreatedObj = Instantiate(effectObj, pos, new());
    }
    public void CreateFX(string name, GameObject host)
    {
        GameObject effectObj;

        effectObj = AssetManager.Instance.GetEffectPrefab(name);
        var CreatedObj = Instantiate(effectObj, host.transform);
    }
}
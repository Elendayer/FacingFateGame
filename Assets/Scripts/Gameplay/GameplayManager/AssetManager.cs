using System.Collections.Generic;
using UnityEngine;

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
    public Sprite IntentionImage(Intention intentions)
    {
        switch (intentions)
        {
            case Intention.None:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_0.png"); 
                case Intention.Damage:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_67.png"); 
                case Intention.Block:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_43.png");
                case Intention.Heal:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_20.png");
                case Intention.Buff:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_24.png");
                case Intention.Debuff:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_25.png");
                case Intention.BuffDebuff:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_24.png");
                case Intention.Other:
                return Resources.Load<Sprite>("Assets / 2D Casual UI / Sprite / GUI_26.png");
        }
        return null;
    }

}
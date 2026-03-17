using facingfate;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.VFX;
using Utility;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance { get; private set; }

    public Sprite HexThin;
    public Sprite HexThick;

    [System.Serializable]
    public struct AssetEntry
    {
        public string name;       // Key name you set in the inspector
        public VisualEffectAsset visualEffectAsset; // Reference to the Visual Effect Asset
    }

    [SerializeField] private List<AssetEntry> effectAssets;

    private Dictionary<string, VisualEffectAsset> effectAssetDict;

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
        effectAssetDict = new Dictionary<string, VisualEffectAsset>();
        foreach (var entry in effectAssets)
        {
            if (!string.IsNullOrEmpty(entry.name) && !effectAssetDict.ContainsKey(entry.name))
            {
                effectAssetDict.Add(entry.name, entry.visualEffectAsset);
            }
        }
    }
    public VisualEffectAsset GetEffectAsset(string name)
    {
        if (effectAssetDict.TryGetValue(name, out VisualEffectAsset fxAsset))
        {
            return fxAsset;
        }

        Debug.LogError($"Prefab with name '{name}' not found!");
        return null;
    }

    public void CreateFxAtPosition(string name, Vector3Int positon, int size = 1)
    {
        (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);

        List<Vector3Int> positions = new List<Vector3Int> { positon };
        Mesh mesh = MeshUtility.GenerateHexMesh(positions);

        vfx.effect.SetInt("_Size", size);
        vfx.effect.SetMesh("_Mesh", mesh);
    }

    public void CreateVFXAtUnifiedPositions(string name, List<Vector3Int> positions)
    {
        (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);
        Mesh mesh = MeshUtility.GenerateHexMesh(positions);

        vfx.effect.SetInt("_Size", positions.Count);
        vfx.effect.SetMesh("_Mesh", mesh);
    }
    public void CreateVFXAtIndividualPositions(string name, List<Vector3Int> positions)
    {
        foreach (Vector3Int pos in positions)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);

            List<Vector3Int> singlePosList = new List<Vector3Int> { pos };
            Mesh mesh = MeshUtility.GenerateHexMesh(singlePosList);

            vfx.effect.SetInt("_Size", 1);
            vfx.effect.SetMesh("_Mesh", mesh);
        }
    }

    public void CreateVFXAttachedToGameObjects(string name, List<GameObject> hosts)
    {
        foreach (GameObject host in hosts)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);
            vfx.obj.transform.SetParent(host.transform);
            vfx.obj.transform.localPosition = Vector3.zero;
        }
    }
    public void CreateVFXAttachedToGameObjects(string name, List<EntityScript> hosts)
    {
        foreach (EntityScript host in hosts)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);
            vfx.obj.transform.SetParent(host.transform);
            vfx.obj.transform.localPosition = Vector3.zero;
        }
    }

    public void CreateVFXAttachedToEntityMesh(string name, GameObject host)
    {
        (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);
        EntityScript entityScript = host.GetComponent<EntityScript>();

        if (entityScript.EntityModel != null)
        {
            if (entityScript.EntityModel.mesh == null)
            {
                Debug.LogWarning("Entity model mesh is not assigned.");
                return;
            }
            Mesh entityMesh = entityScript.EntityModel.mesh;
            vfx.effect.SetMesh("_Mesh", entityMesh);
        }
    }
    public void CreateVFXAttachedToEntityMesh(string name, EntityScript host)
    {
        (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);
        vfx.obj.transform.SetParent(host.transform);
        vfx.obj.transform.localPosition = Vector3.zero;

        if (host.EntityModel != null)
        {
            if (host.EntityModel.mesh == null)
            {
                Debug.LogWarning("Entity model mesh is not assigned.");
                return;
            }
            Mesh entityMesh = host.EntityModel.mesh;
            vfx.effect.SetMesh("_Mesh", entityMesh);
        }
    }
    public (GameObject, VisualEffect) CreateVFX(string name)
    {
        VisualEffectAsset vfxAsset = AssetManager.Instance.GetEffectAsset(name);

        if (vfxAsset != null)
        {
            GameObject vfxObject = new GameObject("VFX_Instance");
            vfxObject.transform.position = transform.position;

            // Add the VisualEffect component and assign the asset
            VisualEffect vfx = vfxObject.AddComponent<VisualEffect>();
            vfx.visualEffectAsset = vfxAsset;

            vfxObject.AddComponent<DestroyVFXAfterEffect>();

            return (vfxObject,vfx);
        }
        else
        {
            Debug.LogWarning("VFX Asset is not assigned.");
            return (null,null);
        }
    }
}
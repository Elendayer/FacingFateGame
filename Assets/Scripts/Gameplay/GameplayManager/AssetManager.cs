using facingfate;
using System.Collections.Generic;
using System.Drawing;
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

    private void ApplyVFXOverrides(VisualEffect effect, VFXOverrides overrides)
    {
        if (effect.HasInt("Size"))
        {
            effect.SetInt("Size", overrides.size);
        }
        if (overrides.count != 0 && effect.HasInt("Count"))
        {
            effect.SetInt("Count", overrides.count);
        }
        if (overrides.mesh != null && effect.HasMesh("Mesh"))
        {
            effect.SetMesh("Mesh", overrides.mesh);
        }
        if (overrides.origin != Vector3.zero && effect.HasVector3("Origin"))
        {
            effect.SetVector3("Origin", overrides.origin);
        }
        if (overrides.direction != Vector3.zero && effect.HasVector3("Direction"))
        {
            effect.SetVector3("Direction", overrides.direction);
        }
    }

    public void CreateFxAtPosition(string name, Vector3Int positon, VFXOverrides overrides)
    {
        (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);

        List<Vector3Int> positions = new List<Vector3Int> { positon };
        Mesh mesh = MeshUtility.GenerateHexMesh(positions);

        ApplyVFXOverrides(vfx.effect, overrides);

    }

    public void CreateVFXAtUnifiedPositions(string name, List<Vector3Int> positions, VFXOverrides overrides)
    {
        (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);
        Mesh mesh = MeshUtility.GenerateHexMesh(positions);

        ApplyVFXOverrides(vfx.effect, new VFXOverrides { size = positions.Count, mesh = mesh });
    }
    public void CreateVFXAtIndividualPositions(string name, List<Vector3Int> positions, VFXOverrides overrides)
    {
        foreach (Vector3Int pos in positions)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);

            vfx.obj.transform.position = TilemapUtilityScript.BaseTilemap.CellToWorld(pos); 

            ApplyVFXOverrides(vfx.effect, overrides);
        }
    }

    public void CreateVFXAttachedToGameObjects(string name, List<GameObject> hosts, VFXOverrides overrides)
    {
        foreach (GameObject host in hosts)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);
            vfx.obj.transform.SetParent(host.transform);
            vfx.obj.transform.localPosition = Vector3.zero;

            ApplyVFXOverrides(vfx.effect, overrides);
        }
    }
    public void CreateVFXAttachedToGameObjects(string name, List<EntityScript> hosts, VFXOverrides overrides)
    {
        foreach (EntityScript host in hosts)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(name);
            vfx.obj.transform.SetParent(host.transform);
            vfx.obj.transform.localPosition = Vector3.zero;

            ApplyVFXOverrides (vfx.effect, overrides);
        }
    }

    public void CreateVFXAttachedToEntityMesh(string name, GameObject host, VFXOverrides overrides)
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

            ApplyVFXOverrides(vfx.effect, new VFXOverrides { mesh = entityMesh });
        }
    }
    public void CreateVFXAttachedToEntityMesh(string name, EntityScript host, VFXOverrides overrides)
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

            ApplyVFXOverrides(vfx.effect, new VFXOverrides { mesh = entityMesh });
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
    public class VFXOverrides
    {
        public int size = 1;
        public int count = 0;

        // For directional effects, these can be used to set the origin and direction of the effect.
        public Vector3 origin = Vector3.zero;
        public Vector3 direction = Vector3.zero;

        public Mesh mesh;
    }
}
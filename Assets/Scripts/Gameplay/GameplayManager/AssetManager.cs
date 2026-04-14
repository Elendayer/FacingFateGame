using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace facingfate
{
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

        private void ApplyVFXData(VisualEffect effect, VFXData overrides)
        {
            if (effect.HasInt("Size"))
            {
                effect.SetInt("Size", overrides.sizeMultiplier);
            }
            if (overrides.activationCount != 0 && effect.HasInt("Count"))
            {
                effect.SetInt("Count", overrides.activationCount);
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

        public void CreateFxAtPosition(VFXData vfxData, Vector3Int positon)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName);

            if (vfx.obj == null || vfx.effect == null)
            {
                Debug.LogError("Failed to create VFX.");
                return;
            }

            List<Vector3> positions = new List<Vector3> { positon };

            ApplyVFXData(vfx.effect, vfxData);

        }

        public void CreateVFXAtUnifiedPositions(VFXData vfxData)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName);

            if (vfx.obj == null || vfx.effect == null)
            {
                Debug.LogError("Failed to create VFX.");
                return;
            }

            ApplyVFXData(vfx.effect, vfxData);
        }
        public void CreateVFXAtIndividualPositions(VFXData vfxData, List<Vector3> positions)
        {
            foreach (Vector3 pos in positions)
            {
                (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName);

                if (vfx.obj == null || vfx.effect == null)
                {
                    Debug.LogError("Failed to create VFX.");
                    return;
                }

                vfx.obj.transform.position = pos;

                ApplyVFXData(vfx.effect, vfxData);
            }
        }

        public void CreateVFXAttachedToGameObjects(VFXData vfxData, List<GameObject> hosts)
        {
            foreach (GameObject host in hosts)
            {
                (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName);

                if (vfx.obj == null || vfx.effect == null)
                {
                    Debug.LogError("Failed to create VFX.");
                    return;
                }

                vfx.obj.transform.SetParent(host.transform);
                vfx.obj.transform.localPosition = Vector3.zero;

                ApplyVFXData(vfx.effect, vfxData);
            }
        }
        public void CreateVFXAttachedToGameObjects(VFXData vfxData, List<EntityScript> hosts)
        {
            foreach (EntityScript host in hosts)
            {
                (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName);

                if (vfx.obj == null || vfx.effect == null)
                {
                    Debug.LogError("Failed to create VFX.");
                    return;
                }

                vfx.obj.transform.SetParent(host.transform);
                vfx.obj.transform.localPosition = Vector3.zero;

                ApplyVFXData(vfx.effect, vfxData);
            }
        }

        public void CreateVFXAttachedToEntityMesh(VFXData vfxData, GameObject host)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName);

            if (vfx.obj == null || vfx.effect == null)
            {
                Debug.LogError("Failed to create VFX.");
                return;
            }

            EntityScript entityScript = host.GetComponent<EntityScript>();

            if (entityScript.EntityModel != null)
            {
                if (entityScript.EntityModel.mesh == null)
                {
                    Debug.LogWarning("Entity model mesh is not assigned.");
                    return;
                }

                ApplyVFXData(vfx.effect, vfxData);
            }
        }
        public void CreateVFXAttachedToEntityMesh(VFXData vfxData, EntityScript host)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName);

            if (vfx.obj == null || vfx.effect == null)
            {
                Debug.LogError("Failed to create VFX.");
                return;
            }

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

                ApplyVFXData(vfx.effect, vfxData);
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

                return (vfxObject, vfx);
            }
            else
            {
                Debug.LogWarning("VFX Asset is not assigned.");
                return (null, null);
            }
        }
    }
    public class VFXData
    {
        public string vfxName;
        public bool attachToMesh = false;

        public int sizeMultiplier => positions.Count;
        public int activationCount = 0;

        public EntityScript Entity;
        public GameObject host;
        public List<Vector3> positions = new List<Vector3>();

        // For directional effects, these can be used to set the origin and direction of the effect.
        public Vector3 origin = Vector3.zero;
        public Vector3 direction = Vector3.zero;

        public Mesh mesh;

        public VFXData(string name, bool attachToMesh = false)
        {
            this.vfxName = name;
            this.attachToMesh = attachToMesh;  
        }
    }
}
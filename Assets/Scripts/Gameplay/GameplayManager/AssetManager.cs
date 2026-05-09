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

        public GameObject CorspePrefab;

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

        public GameObject rangeIndicator;

        public Material EnemyMaterial;
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

            // Create the persistent range indicator instance
            if (rangeIndicator != null)
            {
                activeRangeIndicatorObject = Instantiate(rangeIndicator);
                activeRangeIndicator = activeRangeIndicatorObject.GetComponent<VisualEffect>();

                if (activeRangeIndicator != null)
                {
                    // Remove DestroyVFXAfterEffect if it exists, since this is a persistent indicator
                    DestroyVFXAfterEffect destroyComponent = activeRangeIndicatorObject.GetComponent<DestroyVFXAfterEffect>();
                    if (destroyComponent != null)
                    {
                        Destroy(destroyComponent);
                    }

                    // Remove any existing Canvas or RectTransform that might conflict
                    RectTransform rectTransform = activeRangeIndicatorObject.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        Destroy(rectTransform);
                    }

                    activeRangeIndicatorObject.SetActive(false);
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
            if (effect.HasFloat ("Range"))
            {
                effect.SetFloat("Range", overrides.range);
            }
            if (effect.HasFloat("Area"))
            {
                effect.SetFloat("Area", overrides.area);
            }
            if (effect.HasFloat("Radius"))
            {
                effect.SetFloat("Radius", overrides.radius);
            }
            if (effect.HasInt("Count"))
            {
                effect.SetInt("Count", overrides.activationCount);
            }
            if (effect.HasMesh("Mesh") && overrides.mesh != null)
            {
                effect.SetMesh("Mesh", overrides.mesh);
            }
            if (effect.HasVector3("Start"))
            {
                effect.SetVector3("Start", overrides.start);
            }
            if (effect.HasVector3("End"))
            {
                effect.SetVector3("End", overrides.end);
            }
        }

        public void CreateVFXAtSinglePosition(VFXData vfxData, Vector3 position)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName, vfxData);

            Debug.Log($"Creating {vfx.effect.name} at {vfx.obj.transform.position}");
            if (vfx.obj == null || vfx.effect == null)
            {
                Debug.LogError("Failed to create VFX.");
                return;
            }

            vfx.obj.transform.position = position;
        }
        public void CreateVFXAtIndividualPositions(VFXData vfxData, List<Vector3> positions)
        {
            foreach (Vector3 pos in positions)
            {
                (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName, vfxData);

                if (vfx.obj == null || vfx.effect == null)
                {
                    Debug.LogError("Failed to create VFX.");
                    return;
                }

                vfx.obj.transform.position = pos;
            }
        }

        public void CreateVFXAttachedToGameObject(VFXData vfxData, GameObject host)
        {

            (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName, vfxData);

            if (vfx.obj == null || vfx.effect == null)
            {
                Debug.LogError("Failed to create VFX.");
                return;
            }

            vfx.obj.transform.SetParent(host.transform);
            vfx.obj.transform.localPosition = Vector3.zero;
        }
        public void CreateVFXAttachedToGameObjects(VFXData vfxData, EntityScript host)
        {

            (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName, vfxData);

            if (vfx.obj == null || vfx.effect == null)
            {
                Debug.LogError("Failed to create VFX.");
                return;
            }

            vfx.obj.transform.SetParent(host.transform);
            vfx.obj.transform.localPosition = Vector3.zero;
        }
        

        public void CreateVFXAttachedToEntityMesh(VFXData vfxData, GameObject host)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName, vfxData);

            if (vfx.obj == null || vfx.effect == null)
            {
                Debug.LogError("Failed to create VFX.");
                return;
            }

            EntityScript entityScript = host.GetComponent<EntityScript>();

            if (entityScript.EntityVisual != null)
            {
                if (entityScript.EntityVisual.meshFilter.mesh == null)
                {
                    Debug.LogWarning("Entity model mesh is not assigned.");
                    return;
                }
            }
        }
        public void CreateVFXAttachedToEntityMesh(VFXData vfxData, EntityScript host)
        {
            (GameObject obj, VisualEffect effect) vfx = CreateVFX(vfxData.vfxName, vfxData);

            if (vfx.obj == null || vfx.effect == null)
            {
                Debug.LogError("Failed to create VFX.");
                return;
            }

            vfx.obj.transform.SetParent(host.transform);
            vfx.obj.transform.localPosition = Vector3.zero;

            if (host.EntityVisual != null)
            {
                if (host.EntityVisual.meshFilter.mesh == null)
                {
                    Debug.LogWarning("Entity model mesh is not assigned.");
                    return;
                }
                Mesh entityMesh = host.EntityVisual.meshFilter.mesh;
            }
        }
        public (GameObject, VisualEffect) CreateVFX(string name, VFXData vfxData)
        {
            VisualEffectAsset vfxAsset = AssetManager.Instance.GetEffectAsset(name);

            if (vfxAsset != null)
            {
                GameObject vfxObject = new GameObject("VFX_Instance");

                // Add the VisualEffect component and assign the asset
                VisualEffect vfx = vfxObject.AddComponent<VisualEffect>();
                vfx.visualEffectAsset = vfxAsset;

                vfxObject.AddComponent<DestroyVFXAfterEffect>();
                vfxObject.name = name;

                ApplyVFXData(vfx, vfxData);

                return (vfxObject, vfx);
            }
            else
            {
                Debug.LogWarning("VFX Asset is not assigned.");
                return (null, null);
            }
        }

        private VisualEffect activeRangeIndicator;
        private GameObject activeRangeIndicatorObject;
        private CardData currentRangeIndicatorCard;

        public void ShowRangeIndicator(CardData cardData)
        {
            if (cardData == null || cardData.Owner == null || activeRangeIndicatorObject == null)
            {
                HideRangeIndicator();
                return;
            }

            // If a drag is active, only allow showing the dragging card's range indicator
            DraggableCard activeDrag = DraggableCard.ActiveDraggingCard;
            if (activeDrag != null)
            {
                CardScript dragCardScript = activeDrag.GetComponent<CardScript>();
                if (dragCardScript == null || dragCardScript.cardData != cardData)
                {
                    // Reject this call - a different card is being dragged
                    return;
                }
            }

            bool isSelfTargeting = cardData.targetingData.CardTargetAffiliation == CardTargetAffiliation.Self;

            // If already showing range indicator for this card, just update properties
            if (currentRangeIndicatorCard == cardData && activeRangeIndicatorObject.activeSelf)
            {
                // Update Range property in case it changed
                if (activeRangeIndicator.HasFloat("Range"))
                {
                    activeRangeIndicator.SetFloat("Range", cardData.Range);
                }

                // Update isSelfTargeting
                if (activeRangeIndicator.HasBool("isSelfTargeting"))
                {
                    activeRangeIndicator.SetBool("isSelfTargeting", isSelfTargeting);
                }

                activeRangeIndicator.Reinit();
                activeRangeIndicator.Play();
                return;
            }

            // Parent to caster
            activeRangeIndicatorObject.transform.SetParent(cardData.Owner.transform);
            activeRangeIndicatorObject.transform.localPosition = Vector3.zero;
            activeRangeIndicatorObject.transform.localRotation = Quaternion.identity;

            // Set the Range property
            if (activeRangeIndicator.HasFloat("Range"))
            {
                activeRangeIndicator.SetFloat("Range", cardData.Range);
            }

            // Set isSelfTargeting based on card's target affiliation
            if (activeRangeIndicator.HasBool("isSelfTargeting"))
            {
                activeRangeIndicator.SetBool("isSelfTargeting", isSelfTargeting);
            }

            currentRangeIndicatorCard = cardData;
            activeRangeIndicatorObject.SetActive(true);
            activeRangeIndicator.Reinit();
            activeRangeIndicator.Play();
        }

        public void HideRangeIndicator()
        {
            // If a drag is active, don't hide the indicator
            DraggableCard activeDrag = DraggableCard.ActiveDraggingCard;
            if (activeDrag != null)
            {
                return;
            }

            if (activeRangeIndicatorObject != null)
            {
                activeRangeIndicatorObject.SetActive(false);
            }
            currentRangeIndicatorCard = null;
        }
    }
    public class VFXData
    {
        public string vfxName;
        public bool attachToMesh = false;

        public int activationCount = 0;

        public float range;
        public float area;
        public float radius;

        public EntityScript entity;
        public GameObject host;
        public List<Vector3> positions = new List<Vector3>();

        // For directional effects, these can be used to set the origin and direction of the effect.
        public Vector3 start = Vector3.zero;
        public Vector3 end = Vector3.zero;

        public Mesh mesh;

        public VFXData(string name, bool attachToMesh = false)
        {
            this.vfxName = name;
            this.attachToMesh = attachToMesh;  
        }
    }
}
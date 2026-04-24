using UnityEngine;

namespace facingfate
{
    [CreateAssetMenu(fileName = "EntityAudioProfile", menuName = "FacingFate/Audio/Entity Audio Profile")]
    public class EntityAudioProfile : ScriptableObject
    {
        [Header("Combat")]
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event attackSfx;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event damageSfx;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event blockSfx;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event healSfx;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event deathSfx;

        [Header("Status Effects")]
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event statusAppliedSfx;
        [Tooltip("Optional, empty = silent")] public AK.Wwise.Event modifierExpiredSfx;
    }
}

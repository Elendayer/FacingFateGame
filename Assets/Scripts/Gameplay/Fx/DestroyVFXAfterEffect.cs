using UnityEngine;
using UnityEngine.VFX;

public class DestroyVFXAfterEffect : MonoBehaviour
{
    public float checkInterval = 0.1f; // How often to check particle count
    private VisualEffect vfx;

    private void Start()
    {
        vfx = GetComponent<VisualEffect>();

        if (vfx == null)return;

        // Start checking for completion
        InvokeRepeating("CheckIfFinished", 0.1f, checkInterval);
    }

    private void CheckIfFinished()
    {
        if (vfx.aliveParticleCount <= 0)
        {
            CancelInvoke("CheckIfFinished");
            Destroy(gameObject);
        }
    }
}
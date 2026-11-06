using UnityEngine;

// PULSING EMISSION — animates a set of renderers' emission colour up and down
// over time (a "breathing" glow), instead of a flat static one. Added/driven
// by UpgradeVisuals for any UpgradeTier with pulses = true; stays on the
// GameObject but disabled when not in use, so there's no per-activation
// allocation cost.
public class PulsingEmission : MonoBehaviour
{
    private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float minMultiplier = 0.4f;
    [SerializeField] private float maxMultiplier = 2.0f;

    private Renderer[] targetRenderers;
    private Color[] baseEmissions;

    public void BeginPulsing(Renderer[] renderers)
    {
        targetRenderers = renderers;
        baseEmissions = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Material material = renderers[i].material;
            baseEmissions[i] = material.HasProperty(EmissionColorId)
                ? material.GetColor(EmissionColorId)
                : Color.black;
        }

        enabled = true;
    }

    public void StopPulsing()
    {
        enabled = false;
        targetRenderers = null;
        baseEmissions = null;
    }

    private void Update()
    {
        if (targetRenderers == null) return;

        float wave = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0..1
        float multiplier = Mathf.Lerp(minMultiplier, maxMultiplier, wave);

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] == null) continue;

            Material material = targetRenderers[i].material;
            if (material.HasProperty(EmissionColorId))
                material.SetColor(EmissionColorId, baseEmissions[i] * multiplier);
        }
    }
}

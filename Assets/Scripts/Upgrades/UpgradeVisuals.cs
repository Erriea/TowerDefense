using System.Collections.Generic;
using System.Text;
using UnityEngine;

// UPGRADE VISUALS — shared helper so both Defender and Tower apply the same
// "find the actual model renderers" logic and swap in a dedicated, obviously-
// different "upgraded" material (every slot, every non-VFX renderer) when a
// tier is purchased, plus the matching scale-up.
public static class UpgradeVisuals
{
    // Every renderer that's actually part of the visible model — skips
    // renderers used for VFX only (attack-radius rings, trails, particles),
    // which aren't what the player is looking at and shouldn't get recoloured.
    public static Renderer[] FindModelRenderers(GameObject root)
    {
        List<Renderer> results = new List<Renderer>();

        foreach (Renderer candidate in root.GetComponentsInChildren<Renderer>(true))
        {
            if (candidate is LineRenderer) continue;
            if (candidate is TrailRenderer) continue;
            if (candidate is ParticleSystemRenderer) continue;
            results.Add(candidate);
        }

        Debug.Log($"[UpgradeVisuals] {root.name}: found {results.Count} model renderer(s): " +
            string.Join(", ", BuildNames(results)));

        return results.ToArray();
    }

    private static List<string> BuildNames(List<Renderer> renderers)
    {
        List<string> names = new List<string>();
        foreach (Renderer r in renderers)
            names.Add($"{r.gameObject.name}({r.GetType().Name})");
        return names;
    }

    // Reads each renderer's ORIGINAL (shared, un-swapped) materials before any
    // upgrade is ever applied, so a timed-out upgrade can put back exactly what
    // was there — not just "undo one tint", the true starting look.
    public static Material[][] CaptureBaseMaterials(Renderer[] renderers)
    {
        Material[][] result = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
            result[i] = renderers[i] != null ? renderers[i].sharedMaterials : null;

        return result;
    }

    // Undoes ApplyUpgradeLook: restores every renderer's original material
    // slots and the model's original scale. Called when an upgrade's timer runs out.
    public static void RevertLook(Renderer[] renderers, Material[][] baseMaterials, Transform targetTransform, Vector3 baseScale)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || baseMaterials[i] == null) continue;
            renderers[i].materials = baseMaterials[i];
        }

        if (targetTransform != null)
        {
            targetTransform.localScale = baseScale;
            targetTransform.GetComponent<PulsingEmission>()?.StopPulsing();
        }
    }

    // Applies the "upgraded" look: every material slot on every tracked
    // renderer is replaced with the tier's dedicated override material (a real
    // asset — see Assets/GameAssets/Materials/Resources/UpgradeTierN.mat), and
    // the whole model scales up per the tier.
    public static void ApplyUpgradeLook(Renderer[] renderers, Transform targetTransform, UpgradeTier tier)
    {
        if (tier.overrideMaterial == null)
        {
            Debug.LogWarning($"[UpgradeVisuals] {targetTransform?.name}: tier '{tier.label}' has no overrideMaterial — skipping material swap.");
        }
        else
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;

                int slotCount = renderer.sharedMaterials.Length;
                Material[] swapped = new Material[slotCount];
                for (int i = 0; i < slotCount; i++)
                    swapped[i] = tier.overrideMaterial;

                renderer.materials = swapped;

                Debug.Log($"[UpgradeVisuals] {targetTransform?.name}: swapped {slotCount} material slot(s) on " +
                    $"{renderer.gameObject.name} to '{tier.overrideMaterial.name}'.");
            }
        }

        if (targetTransform != null)
        {
            targetTransform.localScale *= tier.scaleMultiplier;

            if (tier.pulses)
            {
                PulsingEmission pulser = targetTransform.GetComponent<PulsingEmission>();
                if (pulser == null) pulser = targetTransform.gameObject.AddComponent<PulsingEmission>();
                pulser.BeginPulsing(renderers);
            }
            else
            {
                targetTransform.GetComponent<PulsingEmission>()?.StopPulsing();
            }
        }
    }
}

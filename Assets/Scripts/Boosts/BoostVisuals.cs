using System.Collections.Generic;
using UnityEngine;

// BOOST VISUALS — shared helper for both Defender and Tower. Attack Speed
// swaps in a dedicated glow material (plus scale-up and optional pulsing),
// same mechanism the old upgrade system used. Shield instead wraps the model
// in a translucent blue bubble, since it has to be able to show alongside an
// Attack Speed boost at the same time without the two fighting over the same
// material slots.
public static class BoostVisuals
{
    private const string ShieldBubbleName = "ShieldBubble (generated)";
    private static Material sharedShieldMaterial;

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

        return results.ToArray();
    }

    // Reads each renderer's ORIGINAL (shared, un-swapped) materials before any
    // boost is ever applied, so an Attack Speed boost ending can put back
    // exactly what was there.
    public static Material[][] CaptureBaseMaterials(Renderer[] renderers)
    {
        Material[][] result = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
            result[i] = renderers[i] != null ? renderers[i].sharedMaterials : null;

        return result;
    }

    // ---- Attack Speed look ----

    public static void ApplyAttackSpeedLook(Renderer[] renderers, Transform targetTransform, BoostDefinition definition)
    {
        if (definition.overrideMaterial != null)
        {
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;

                int slotCount = renderer.sharedMaterials.Length;
                Material[] swapped = new Material[slotCount];
                for (int i = 0; i < slotCount; i++)
                    swapped[i] = definition.overrideMaterial;

                renderer.materials = swapped;
            }
        }

        if (targetTransform == null) return;

        targetTransform.localScale *= definition.scaleMultiplier;

        if (definition.pulses)
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

    public static void RevertAttackSpeedLook(Renderer[] renderers, Material[][] baseMaterials, Transform targetTransform, Vector3 baseScale)
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null || baseMaterials[i] == null) continue;
            renderers[i].materials = baseMaterials[i];
        }

        if (targetTransform == null) return;

        targetTransform.localScale = baseScale;
        targetTransform.GetComponent<PulsingEmission>()?.StopPulsing();
    }

    // ---- Shield look ----

    public static void ApplyShield(Transform targetTransform, Renderer[] modelRenderers)
    {
        GameObject bubble = FindOrCreateShieldBubble(targetTransform, modelRenderers);
        bubble.SetActive(true);
    }

    public static void RemoveShield(Transform targetTransform)
    {
        Transform existing = targetTransform.Find(ShieldBubbleName);
        if (existing != null) existing.gameObject.SetActive(false);
    }

    private static GameObject FindOrCreateShieldBubble(Transform targetTransform, Renderer[] modelRenderers)
    {
        Transform existing = targetTransform.Find(ShieldBubbleName);
        if (existing != null) return existing.gameObject;

        GameObject bubble = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bubble.name = ShieldBubbleName;

        Collider col = bubble.GetComponent<Collider>();
        if (col != null) Object.Destroy(col);

        MeshRenderer meshRenderer = bubble.GetComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.material = GetShieldMaterial();

        bubble.transform.SetParent(targetTransform, worldPositionStays: false);

        Bounds bounds = CombineBounds(modelRenderers, targetTransform);
        bubble.transform.position = bounds.center;

        float worldDiameter = bounds.extents.magnitude * 2.4f;
        Vector3 parentScale = targetTransform.lossyScale;
        bubble.transform.localScale = new Vector3(
            worldDiameter / Mathf.Max(0.0001f, parentScale.x),
            worldDiameter / Mathf.Max(0.0001f, parentScale.y),
            worldDiameter / Mathf.Max(0.0001f, parentScale.z));

        return bubble;
    }

    private static Bounds CombineBounds(Renderer[] renderers, Transform fallback)
    {
        Bounds combined = default;
        bool started = false;

        foreach (Renderer r in renderers)
        {
            if (r == null) continue;
            if (!started) { combined = r.bounds; started = true; }
            else combined.Encapsulate(r.bounds);
        }

        if (!started) combined = new Bounds(fallback.position, Vector3.one);

        return combined;
    }

    // One shared translucent-blue material, built at runtime off the same URP
    // shader every other boost material uses, so there's no handwritten
    // transparent .mat asset to get wrong.
    private static Material GetShieldMaterial()
    {
        if (sharedShieldMaterial != null) return sharedShieldMaterial;

        Material reference = Resources.Load<Material>("GolemTier2");
        Shader shader = reference != null ? reference.shader : Shader.Find("Universal Render Pipeline/Lit");

        Material material = new Material(shader) { name = "ShieldBubble (generated)" };

        material.SetFloat("_Surface", 1f); // Transparent
        material.SetFloat("_Blend", 0f);   // Alpha
        material.SetFloat("_Cull", 0f);    // render both faces — the camera can end up inside the bubble
        material.SetFloat("_ZWrite", 0f);
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        Color tint = new Color(0.35f, 0.7f, 1f, 0.35f);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", tint);
        if (material.HasProperty("_Color")) material.SetColor("_Color", tint);

        if (material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.3f, 0.8f, 1.6f));
        }

        sharedShieldMaterial = material;
        return sharedShieldMaterial;
    }
}

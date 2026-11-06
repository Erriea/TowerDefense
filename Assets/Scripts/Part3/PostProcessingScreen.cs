using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingScreen : MonoBehaviour
{
    [SerializeField] private Volume damageVolume;

    [SerializeField] private float screenDuration = 0.5f;
    [SerializeField] private float vignetteIntensity = 0.2f;

    // A quick bright white flash for a Lightning Strike environmental event — no vignette
    // (this isn't a pain reaction like the damage effect), just a brief overexposed flash.
    // Driven mostly by postExposure (a real camera-exposure spike), not colorFilter alone,
    // because colorFilter's tint-only change was barely visible against a bright outdoor scene.
    [SerializeField] private float lightningFlashDuration = 0.18f;
    [SerializeField] private float lightningExposureSpike = 4f;

    // A held grey-blue haze for the whole duration of a Fog environmental event, rather than a
    // one-shot flash. Reasserted every frame in Update() (while no flash coroutine is mid-flight)
    // instead of being set once by a coroutine, so a Lightning flash or the damage effect firing
    // mid-fog can't permanently wipe the fog tint out early — it just reappears next frame.
    [SerializeField] private float fogVignetteIntensity = 0.15f;
    [SerializeField] private Color fogColorFilter = new Color(0.78f, 0.8f, 0.82f);

    // A faint rhythmic vignette pulse for the whole duration of a Wind environmental event —
    // cheap reuse of the same Volume/Update() machinery built for Fog, just a sine wave instead
    // of a flat hold, so Wind reads as restless rather than static.
    [SerializeField] private float windVignettePulseIntensity = 0.12f;
    [SerializeField] private float windVignettePulseSpeed = 2.5f;

    private bool fogActive;
    private bool windActive;

    private Vignette vignette;
    private ColorAdjustments colourAdjustments;
    private DepthOfField depthOfField;

    private Coroutine screenCoroutine;

    private void Awake()
    {
        damageVolume.profile.TryGet<Vignette>(out vignette);
        damageVolume.profile.TryGet<ColorAdjustments>(out colourAdjustments);
        damageVolume.profile.TryGet<DepthOfField>(out depthOfField);
        
        vignette.intensity.value = 0;
        colourAdjustments.colorFilter.value = Color.white;

        if (depthOfField != null)
        {
            // Gaussian mode with a near-zero start/end turns it into a flat full-screen blur
            // rather than a camera-focus effect — exactly what "the screen goes blurry" needs.
            // Only the on/off switch (active) changes at runtime; these shape values are fixed.
            depthOfField.mode.value = DepthOfFieldMode.Gaussian;
            depthOfField.gaussianStart.value = 0f;
            depthOfField.gaussianEnd.value = 3f;
            depthOfField.active = false;
        }
    }
    
    public void PlayDamageEffect()
    {
        if (screenCoroutine != null)
        {
            StopCoroutine(screenCoroutine);
        }

        screenCoroutine = StartCoroutine(DamageEffect());
    }

    private IEnumerator DamageEffect()
    {
        vignette.intensity.value = vignetteIntensity;
        colourAdjustments.colorFilter.value =
            new Color(1f, 0.65f, 0.65f);

        yield return new WaitForSeconds(screenDuration);

        vignette.intensity.value = 0;
        colourAdjustments.colorFilter.value = Color.white;

        screenCoroutine = null;
    }

    public void PlayLightningFlash()
    {
        if (screenCoroutine != null)
        {
            StopCoroutine(screenCoroutine);
        }

        screenCoroutine = StartCoroutine(LightningFlashEffect());
    }

    private IEnumerator LightningFlashEffect()
    {
        colourAdjustments.postExposure.value = lightningExposureSpike;
        colourAdjustments.colorFilter.value = new Color(1.3f, 1.3f, 1.1f);

        yield return new WaitForSeconds(lightningFlashDuration);

        colourAdjustments.postExposure.value = 0f;
        colourAdjustments.colorFilter.value = Color.white;

        screenCoroutine = null;
    }

    // Turns the Fog haze on or off. Called once at the start and once at the end of a Fog
    // event by EnvironmentalEventManager — the actual holding/reasserting happens in Update().
    public void SetFogActive(bool active)
    {
        fogActive = active;

        if (depthOfField != null)
            depthOfField.active = active;

        if (!active && screenCoroutine == null && !windActive)
        {
            vignette.intensity.value = 0f;
            colourAdjustments.colorFilter.value = Color.white;
        }
    }

    // Turns the Wind vignette pulse on or off. Called once at the start and once at the end of
    // a Wind event by EnvironmentalEventManager, same pattern as SetFogActive.
    public void SetWindActive(bool active)
    {
        windActive = active;

        if (!active && screenCoroutine == null && !fogActive)
        {
            vignette.intensity.value = 0f;
        }
    }

    private void Update()
    {
        // Don't fight a flash that's actively mid-play (damage effect / lightning) — let it
        // finish, then resume holding the fog/wind look on the frame right after.
        if (screenCoroutine != null) return;

        if (fogActive)
        {
            vignette.intensity.value = fogVignetteIntensity;
            colourAdjustments.colorFilter.value = fogColorFilter;
        }
        else if (windActive)
        {
            float pulse = (Mathf.Sin(Time.time * windVignettePulseSpeed) + 1f) * 0.5f;
            vignette.intensity.value = pulse * windVignettePulseIntensity;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessingScreen : MonoBehaviour
{
    [SerializeField] private Volume damageVolume;

    [SerializeField] private float screenDuration = 0.5f;
    [SerializeField] private float vignetteIntensity = 0.2f;
    
    private Vignette vignette;
    private ColorAdjustments colourAdjustments;
    
    private Coroutine screenCoroutine;

    private void Awake()
    {
        damageVolume.profile.TryGet<Vignette>(out vignette);
        damageVolume.profile.TryGet<ColorAdjustments>(out colourAdjustments);
        
        vignette.intensity.value = 0;
        colourAdjustments.colorFilter.value = Color.white;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

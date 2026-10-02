using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Drop this on the Tower, Defender, and Enemy prefabs for a cheap hit-flash + death pop.
// Flashes EVERY renderer/material under this object, so multi-material models
// (like the crow's Feathers/LighterFeather/Beak/Eye split) flash fully, not just one part.
public class HitFeedback : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private float deathPopDuration = 0.15f;

    private Renderer[] renderers;
    private Color[] originalColors;
    private Coroutine flashRoutine;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);

        List<Color> colors = new List<Color>();
        foreach (var rend in renderers)
        {
            foreach (var mat in rend.materials)
            {
                colors.Add(mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor") : mat.color);
            }
        }
        originalColors = colors.ToArray();
    }

    public void Flash()
    {
        if (renderers == null || renderers.Length == 0) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        SetAllMaterialColors(flashColor);
        yield return new WaitForSeconds(flashDuration);
        RestoreAllMaterialColors();
        flashRoutine = null;
    }

    private void SetAllMaterialColors(Color color)
    {
        foreach (var rend in renderers)
        {
            foreach (var mat in rend.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);
                else
                    mat.color = color;
            }
        }
    }

    private void RestoreAllMaterialColors()
    {
        int colorIndex = 0;
        foreach (var rend in renderers)
        {
            foreach (var mat in rend.materials)
            {
                Color original = originalColors[colorIndex];

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", original);
                else
                    mat.color = original;

                colorIndex++;
            }
        }
    }

    public void Die(Action onComplete)
    {
        StartCoroutine(DeathPopRoutine(onComplete));
    }

    private IEnumerator DeathPopRoutine(Action onComplete)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < deathPopDuration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / deathPopDuration);
            yield return null;
        }

        onComplete?.Invoke();
    }
}

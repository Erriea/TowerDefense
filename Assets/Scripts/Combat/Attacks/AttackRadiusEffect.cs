using System.Collections;
using UnityEngine;

// ATTACK RADIUS EFFECT — draws a brief expanding ring on the ground to show
// an attack's range the moment it fires. Reusable on any attacker (Golem,
// Tower, Archer later) — just call Play(range) from its attack code.
[RequireComponent(typeof(LineRenderer))]
public class AttackRadiusEffect : MonoBehaviour
{
    [SerializeField] private int segments = 48;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private Color ringColor = new Color(1f, 0.3f, 0.1f, 0.8f);
    [SerializeField] private float effectDuration = 0.3f;
    [SerializeField] private float yOffset = 0.05f;

    private LineRenderer lineRenderer;
    private Coroutine playRoutine;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = false;
        lineRenderer.positionCount = segments;
        lineRenderer.widthMultiplier = lineWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.enabled = false;
    }

    public void Play(float radius)
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayRoutine(radius));
    }

    private IEnumerator PlayRoutine(float targetRadius)
    {
        lineRenderer.enabled = true;
        float elapsed = 0f;

        while (elapsed < effectDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / effectDuration;

            float currentRadius = Mathf.Lerp(0f, targetRadius, t);
            Color currentColor = ringColor;
            currentColor.a = Mathf.Lerp(ringColor.a, 0f, t);

            DrawCircle(currentRadius);
            lineRenderer.startColor = currentColor;
            lineRenderer.endColor = currentColor;

            yield return null;
        }

        lineRenderer.enabled = false;
        playRoutine = null;
    }

    private void DrawCircle(float radius)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            lineRenderer.SetPosition(i, new Vector3(x, yOffset, z));
        }
    }
}

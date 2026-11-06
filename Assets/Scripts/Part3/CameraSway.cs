using UnityEngine;

// CAMERA SWAY — subtle camera motion for the Wind environmental event. The camera leans in the
// same direction projectiles are drifting (so what you see matches what's happening to your
// shots), plus a small amount of smooth Perlin-noise wobble on top so it reads as "wind" rather
// than a rigid tilt. Eases in/out over time instead of snapping, so it doesn't feel like a jump
// cut when Wind starts or ends. Uses Camera.main, the same camera-reference pattern already used
// elsewhere in this project (DefenderPlacementGenerator, EnemyHealthBar).
public class CameraSway : MonoBehaviour
{
    [Header("Lean (toward the wind direction)")]
    [SerializeField] private float leanAmount = 0.35f;
    [SerializeField] private float rollDegrees = 3f;
    [SerializeField] private float pitchDegrees = 2f;

    [Header("Wobble (on top of the lean)")]
    [SerializeField] private float jitterAmount = 0.08f;
    [SerializeField] private float jitterSpeed = 0.6f;

    [Header("Ease")]
    [SerializeField] private float swayLerpSpeed = 1.5f;

    private Camera targetCamera;
    private Vector3 basePosition;
    private Quaternion baseRotation;
    private bool initialised;

    private bool active;
    private Vector3 leanDirection = Vector3.forward;
    private float currentStrength;
    private float targetStrength;

    private float noiseSeedX;
    private float noiseSeedZ;

    private void Awake()
    {
        noiseSeedX = Random.Range(0f, 1000f);
        noiseSeedZ = Random.Range(0f, 1000f);
    }

    private void EnsureCamera()
    {
        if (initialised) return;

        targetCamera = Camera.main;
        if (targetCamera == null) return;

        basePosition = targetCamera.transform.localPosition;
        baseRotation = targetCamera.transform.localRotation;
        initialised = true;
    }

    // direction: the same horizontal wind-drift vector EnvironmentalEventManager feeds into
    // Projectile.SetWind(), so the camera visibly leans the way shots are being blown.
    public void StartSway(Vector3 direction)
    {
        EnsureCamera();
        if (!initialised) return;

        if (direction.sqrMagnitude > 0.0001f)
            leanDirection = direction.normalized;

        active = true;
        targetStrength = 1f;
    }

    public void StopSway()
    {
        active = false;
        targetStrength = 0f;
    }

    private void LateUpdate()
    {
        if (!initialised) return;
        if (!active && currentStrength <= 0.001f) return;

        currentStrength = Mathf.MoveTowards(currentStrength, targetStrength, swayLerpSpeed * Time.deltaTime);

        if (currentStrength <= 0.001f && !active)
        {
            targetCamera.transform.localPosition = basePosition;
            targetCamera.transform.localRotation = baseRotation;
            return;
        }

        float noiseX = (Mathf.PerlinNoise(Time.time * jitterSpeed + noiseSeedX, 0f) - 0.5f) * 2f;
        float noiseZ = (Mathf.PerlinNoise(0f, Time.time * jitterSpeed + noiseSeedZ) - 0.5f) * 2f;

        Vector3 lean = leanDirection * leanAmount;
        Vector3 jitter = new Vector3(noiseX, 0f, noiseZ) * jitterAmount;
        Vector3 offset = (lean + jitter) * currentStrength;

        float roll = -leanDirection.x * rollDegrees * currentStrength;
        float pitch = leanDirection.z * pitchDegrees * currentStrength;

        targetCamera.transform.localPosition = basePosition + offset;
        targetCamera.transform.localRotation = baseRotation * Quaternion.Euler(pitch, 0f, roll);
    }
}

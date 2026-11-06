using System.Collections;
using UnityEngine;

// ENVIRONMENTAL EVENT MANAGER — procedurally rolls battlefield-wide hazard events as waves
// progress, so no two playthroughs get the same sequence of events: Lightning Strike (stuns
// enemies near a random live target), Fog (shrinks every defender's targeting range for a
// while), and Wind (makes in-flight projectiles curve and occasionally miss). Only one event
// runs at a time — a new wave never rolls again while one is already active.
public class EnvironmentalEventManager : MonoBehaviour
{
    [Header("Trigger")]
    [SerializeField] private int firstEligibleWave = 4;
    [SerializeField, Range(0f, 1f)] private float triggerChancePerWave = 0.4f;

    [Header("Lightning Strike")]
    [SerializeField] private float lightningRadius = 8f;
    [SerializeField] private float lightningStunDuration = 2.5f;
    [SerializeField] private GameObject lightningVfxPrefab; // optional — safe to leave empty, just skips the extra VFX

    [Header("Fog")]
    [SerializeField, Range(0f, 1f)] private float fogRangeMultiplier = 0.55f;
    [SerializeField] private float fogDuration = 8f;

    [Header("Wind")]
    [SerializeField] private float windDriftStrength = 3f;
    [SerializeField] private float windDuration = 8f;

    [Header("TEST OVERRIDE — leave all false; set ONE true to force-test that event, then set back")]
    [SerializeField] private bool debugForceLightning = false;
    [SerializeField] private bool debugForceFog = false;
    [SerializeField] private bool debugForceWind = false;

    private WaveManager waveManager;
    private PostProcessingScreen screenEffect;
    private CameraSway cameraSway;
    private bool eventInProgress;

    private void Awake()
    {
        waveManager = FindFirstObjectByType<WaveManager>();
        screenEffect = FindFirstObjectByType<PostProcessingScreen>();
        cameraSway = FindFirstObjectByType<CameraSway>();
    }

    private void OnEnable()
    {
        if (waveManager != null)
            waveManager.OnWaveStarted += HandleWaveStarted;
    }

    private void OnDisable()
    {
        if (waveManager != null)
            waveManager.OnWaveStarted -= HandleWaveStarted;
    }

    private void HandleWaveStarted(int waveNumber)
    {
        if (eventInProgress) return;

        // Test-only shortcuts: skip the wave-number/chance gate and force one specific event
        // every wave. Turn these back off in the Inspector once you're done testing.
        if (debugForceLightning)
        {
            TriggerLightningStrike();
            return;
        }

        if (debugForceFog)
        {
            StartCoroutine(RunFog());
            return;
        }

        if (debugForceWind)
        {
            StartCoroutine(RunWind());
            return;
        }

        if (waveNumber < firstEligibleWave) return;
        if (Random.value > triggerChancePerWave) return;

        switch (Random.Range(0, 3))
        {
            case 0: TriggerLightningStrike(); break;
            case 1: StartCoroutine(RunFog()); break;
            case 2: StartCoroutine(RunWind()); break;
        }
    }

    // Strikes near a randomly-chosen LIVE enemy rather than a blind random world point, so the
    // event is always visibly relevant instead of occasionally hitting empty ground.
    private void TriggerLightningStrike()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        if (enemies.Length == 0) return;

        Vector3 strikeCenter = enemies[Random.Range(0, enemies.Length)].transform.position;

        Collider[] hits = Physics.OverlapSphere(strikeCenter, lightningRadius);
        int stunnedCount = 0;

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.Stun(lightningStunDuration);
                stunnedCount++;
            }
        }

        AudioManager.PlayExplosion(strikeCenter);
        screenEffect?.PlayLightningFlash();

        if (lightningVfxPrefab != null)
            Instantiate(lightningVfxPrefab, strikeCenter, Quaternion.identity);

        Debug.Log($"Lightning Strike hit near {strikeCenter}, stunned {stunnedCount} enemies for {lightningStunDuration}s");
    }

    // Shrinks every defender's targeting range for fogDuration seconds, then restores it.
    // Re-queries defenders at the end (not just the start) in case any were placed or lost
    // mid-fog, so a defender placed partway through still gets its range restored correctly.
    private IEnumerator RunFog()
    {
        eventInProgress = true;

        foreach (var shooter in FindObjectsByType<ProjectileShooter>(FindObjectsSortMode.None))
            shooter.SetEnvironmentalRangeMultiplier(fogRangeMultiplier);

        screenEffect?.SetFogActive(true);
        AudioManager.PlayFog();
        Debug.Log($"Fog rolled in — defender range cut to {fogRangeMultiplier:P0} for {fogDuration}s");

        yield return new WaitForSeconds(fogDuration);

        foreach (var shooter in FindObjectsByType<ProjectileShooter>(FindObjectsSortMode.None))
            shooter.SetEnvironmentalRangeMultiplier(1f);

        screenEffect?.SetFogActive(false);
        eventInProgress = false;
    }

    // Picks one random horizontal gust direction and holds it for windDuration seconds, making
    // every projectile currently and newly in flight drift off its straight-line aim.
    private IEnumerator RunWind()
    {
        eventInProgress = true;

        Vector2 randomDirection2D = Random.insideUnitCircle.normalized;
        Vector3 drift = new Vector3(randomDirection2D.x, 0f, randomDirection2D.y) * windDriftStrength;

        Projectile.SetWind(drift);
        cameraSway?.StartSway(drift);
        screenEffect?.SetWindActive(true);
        AudioManager.PlayWind();
        Debug.Log($"Wind picked up — projectiles drifting toward {drift} for {windDuration}s");

        yield return new WaitForSeconds(windDuration);

        Projectile.SetWind(Vector3.zero);
        cameraSway?.StopSway();
        screenEffect?.SetWindActive(false);
        eventInProgress = false;
    }
}

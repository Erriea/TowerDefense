using UnityEngine;

// AUDIO MANAGER — one place for every sound effect and the background music track.
// Put this on a single GameObject in the scene (it persists across scene loads) and
// assign each clip in the Inspector. Anything else in the project plays a sound by
// calling the static AudioManager.PlayX(...) methods instead of needing its own
// AudioSource/AudioClip fields — Tower, Defender, Enemy, ProjectileShooter and the
// UI buttons all just call into here.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip bgMusic;
    [SerializeField] private float musicVolume = 0.25f;

    [Header("SFX")]
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip shotClip;
    [SerializeField] private AudioClip towerCrackClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip defenderPlaceClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private float sfxVolume = 1f;

    // A per-clip multiplier on top of sfxVolume, for sounds that need their own loudness
    // without changing every other SFX — explosions should hit harder than a regular shot.
    [SerializeField] private float explosionVolumeMultiplier = 1.8f;

    [Header("Environmental Events")]
    [SerializeField] private AudioClip windClip;
    [SerializeField] private AudioClip fogClip;

    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = GetComponent<AudioSource>();
        musicSource.clip = bgMusic;
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.spatialBlend = 0f; // music and UI one-shots play flat, no 3D falloff
        musicSource.playOnAwake = false;
        musicSource.Play();

        Debug.Log(bgMusic == null
            ? "[AudioManager] Awake: bgMusic clip is NOT assigned!"
            : $"[AudioManager] Awake: playing '{bgMusic.name}', isPlaying={musicSource.isPlaying}, volume={musicSource.volume}");
    }

    private static void PlayAtPoint(AudioClip clip, Vector3 position, float volumeMultiplier = 1f)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[AudioManager] PlayAtPoint called but Instance is null — is the AudioManager GameObject in the scene and active?");
            return;
        }
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] PlayAtPoint called but the clip field is unassigned.");
            return;
        }
        Debug.Log($"[AudioManager] Playing '{clip.name}' at {position}");
        // Not clamped to 1 here on purpose: sfxVolume already sits at its max (1), so a
        // multiplier above 1 (e.g. explosionVolumeMultiplier) needs to actually push past that
        // baseline to read as louder, rather than being clamped straight back down to it.
        AudioSource.PlayClipAtPoint(clip, position, Instance.sfxVolume * volumeMultiplier);
    }

    // Melee attacks — Golem, Bulwark/Racoon, and the base Enemy attack tick (Musher).
    public static void PlayAttack(Vector3 position) => PlayAtPoint(Instance != null ? Instance.attackClip : null, position);

    // Any projectile being fired — Tower, Spitter, Archer all share ProjectileShooter.FireAt().
    public static void PlayShot(Vector3 position) => PlayAtPoint(Instance != null ? Instance.shotClip : null, position);

    // The tower taking a hit.
    public static void PlayTowerCrack(Vector3 position) => PlayAtPoint(Instance != null ? Instance.towerCrackClip : null, position);

    // A defender being placed at a confirmed spot.
    public static void PlayDefenderPlace(Vector3 position) => PlayAtPoint(Instance != null ? Instance.defenderPlaceClip : null, position);

    // A Bomber detonating, or an environmental event's impact (e.g. Lightning Strike) — plays
    // louder than regular SFX via explosionVolumeMultiplier so it actually reads as an impact.
    public static void PlayExplosion(Vector3 position) =>
        PlayAtPoint(Instance != null ? Instance.explosionClip : null, position, Instance != null ? Instance.explosionVolumeMultiplier : 1f);

    // Wind and Fog are battlefield-wide atmospheric events, not tied to one point in space, so
    // they play non-positionally through the same flat 2D source as music/UI clicks, rather
    // than PlayAtPoint's 3D falloff (which would make them quieter depending on camera position).
    public static void PlayWind()
    {
        if (Instance == null || Instance.windClip == null) return;
        Instance.musicSource.PlayOneShot(Instance.windClip, Instance.sfxVolume);
    }

    public static void PlayFog()
    {
        if (Instance == null || Instance.fogClip == null) return;
        Instance.musicSource.PlayOneShot(Instance.fogClip, Instance.sfxVolume);
    }

    // Any UI button interaction. Non-positional — plays through the shared 2D music source.
    public static void PlayButtonClick()
    {
        if (Instance == null)
        {
            Debug.LogWarning("[AudioManager] PlayButtonClick called but Instance is null.");
            return;
        }
        if (Instance.buttonClickClip == null)
        {
            Debug.LogWarning("[AudioManager] PlayButtonClick called but buttonClickClip is unassigned.");
            return;
        }
        Debug.Log("[AudioManager] Playing button click");
        Instance.musicSource.PlayOneShot(Instance.buttonClickClip, Instance.sfxVolume);
    }
}

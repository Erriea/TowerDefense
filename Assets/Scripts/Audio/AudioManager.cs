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
    [SerializeField] private float musicVolume = 0.5f;

    [Header("SFX")]
    [SerializeField] private AudioClip attackClip;
    [SerializeField] private AudioClip shotClip;
    [SerializeField] private AudioClip towerCrackClip;
    [SerializeField] private AudioClip buttonClickClip;
    [SerializeField] private AudioClip defenderPlaceClip;
    [SerializeField] private AudioClip explosionClip;
    [SerializeField] private float sfxVolume = 1f;

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

    private static void PlayAtPoint(AudioClip clip, Vector3 position)
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
        AudioSource.PlayClipAtPoint(clip, position, Instance.sfxVolume);
    }

    // Melee attacks — Golem, Bulwark/Racoon, and the base Enemy attack tick (Musher).
    public static void PlayAttack(Vector3 position) => PlayAtPoint(Instance != null ? Instance.attackClip : null, position);

    // Any projectile being fired — Tower, Spitter, Archer all share ProjectileShooter.FireAt().
    public static void PlayShot(Vector3 position) => PlayAtPoint(Instance != null ? Instance.shotClip : null, position);

    // The tower taking a hit.
    public static void PlayTowerCrack(Vector3 position) => PlayAtPoint(Instance != null ? Instance.towerCrackClip : null, position);

    // A defender being placed at a confirmed spot.
    public static void PlayDefenderPlace(Vector3 position) => PlayAtPoint(Instance != null ? Instance.defenderPlaceClip : null, position);

    // A Bomber detonating.
    public static void PlayExplosion(Vector3 position) => PlayAtPoint(Instance != null ? Instance.explosionClip : null, position);

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

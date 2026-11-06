using System;
using UnityEngine;

public class Tower : MonoBehaviour, IDamageable, IBoostable
{
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private DamageTextSpawner damageTextSpawner;

    private float currentHealth;
    private bool isDead;
    private bool isShielded;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public event Action<float, float> OnHealthChanged;
    public static event Action OnTowerDestroyed;

    public static event Action<Tower> OnTowerSpawned;
    
    [SerializeField] private PostProcessingScreen screenEffect;

    // Captured once in Awake, before any boost is ever applied — what an
    // expired Attack Speed boost reverts back to.
    private Renderer[] visualRenderers;
    private Material[][] baseMaterials;
    private Vector3 baseScale;

    private void Awake()
    {
        screenEffect = FindFirstObjectByType<PostProcessingScreen>();
        currentHealth = maxHealth;
        OnTowerSpawned?.Invoke(this);

        visualRenderers = BoostVisuals.FindModelRenderers(gameObject);
        baseMaterials = BoostVisuals.CaptureBaseMaterials(visualRenderers);
        baseScale = transform.localScale;
        
        damageTextSpawner = GetComponent<DamageTextSpawner>();
        ApplyAlreadyActiveBoosts();
    }

    private void OnEnable()
    {
        BoostManager.OnBoostActivated += HandleBoostActivated;
        BoostManager.OnBoostEnded += HandleBoostEnded;
    }

    private void OnDisable()
    {
        BoostManager.OnBoostActivated -= HandleBoostActivated;
        BoostManager.OnBoostEnded -= HandleBoostEnded;
    }

    private void HandleBoostActivated(DefenderKind kind, BoostType type, BoostDefinition definition)
    {
        if (kind == DefenderKind.Tower)
            ApplyBoost(type, definition);
    }

    private void HandleBoostEnded(DefenderKind kind, BoostType type)
    {
        if (kind == DefenderKind.Tower)
            RevertBoost(type);
    }

    // Picks up whichever of the Tower's two boosts (Shield, Attack Speed) are
    // already running globally — keeps the Tower in sync if it's re-enabled
    // or re-spawned while a boost is mid-flight.
    private void ApplyAlreadyActiveBoosts()
    {
        if (BoostManager.Instance == null) return;

        if (BoostManager.Instance.IsActive(DefenderKind.Tower, BoostType.Shield))
            ApplyBoost(BoostType.Shield, BoostManager.Instance.GetDefinition(DefenderKind.Tower, BoostType.Shield));

        if (BoostManager.Instance.IsActive(DefenderKind.Tower, BoostType.AttackSpeed))
            ApplyBoost(BoostType.AttackSpeed, BoostManager.Instance.GetDefinition(DefenderKind.Tower, BoostType.AttackSpeed));
    }

    public void ApplyBoost(BoostType type, BoostDefinition definition)
    {
        if (definition == null) return;

        if (type == BoostType.Shield)
        {
            isShielded = true;
            BoostVisuals.ApplyShield(transform, visualRenderers);
        }
        else if (type == BoostType.AttackSpeed)
        {
            GetComponent<ProjectileShooter>()?.ApplyStatBonus(definition.damageBonus, 0f, definition.fireIntervalReduction);
            BoostVisuals.ApplyAttackSpeedLook(visualRenderers, transform, definition);
        }

        Debug.Log($"Tower activated {definition.label}.");
    }

    // Called when the Tower's boost timer runs out — undoes exactly that one
    // boost's effect, leaving the other boost (if active) untouched.
    public void RevertBoost(BoostType type)
    {
        if (type == BoostType.Shield)
        {
            isShielded = false;
            BoostVisuals.RemoveShield(transform);
        }
        else if (type == BoostType.AttackSpeed)
        {
            GetComponent<ProjectileShooter>()?.ResetToBase();
            BoostVisuals.RevertAttackSpeedLook(visualRenderers, baseMaterials, transform, baseScale);
        }

        Debug.Log($"Tower's {type} boost ended.");
    }

    private void Start()
    {
        Debug.Log("TOWER SCRIPT IS RUNNING!");
    }

    public void TakeDamage(float amount)
    {
        Debug.Log("TOWER TAKE DAMAGE CALLED!");
        if (isDead || isShielded) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        Debug.Log($"Tower took {amount} damage, {currentHealth} / {maxHealth} HP left");
        AudioManager.PlayTowerCrack(transform.position);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        GetComponent<HitFeedback>()?.Flash();
        
        screenEffect?.PlayDamageEffect();
        damageTextSpawner.SpawnDamageText();
        
        if (damageTextSpawner != null)
        {
            damageTextSpawner.SpawnDamageText();
        }
        else
        {
            Debug.LogError("DamageTextSpawner is missing from the Tower!");
        }
        
        if (currentHealth <= 0f)
        {
            Die();
            // isDead = true;
            //
            // //spawn destruction particles and debris
            // GetComponent<TowerDestruction>()?.DestroyTower();
            //
            // //Notify other systems
            // OnTowerDestroyed?.Invoke();
            //
            // //Destroys the original tower
            // var hitFeedback = GetComponent<HitFeedback>();
            //
            // if (hitFeedback != null)
            // {
            //     hitFeedback.Die(() => Destroy(gameObject));
            // }
            // else
            // {
            //     Destroy(gameObject);
            // }
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("TOWER HAS DIED!");

        // Spawn destruction effects BEFORE destroying tower
        TowerDestruction destruction =
            GetComponent<TowerDestruction>();

        if (destruction != null)
        {
            destruction.DestroyTower();
        }
        else
        {
            Debug.LogError("TowerDestruction component is missing!");
        }

        OnTowerDestroyed?.Invoke();

        // Destroy the original tower
        var hitFeedback = GetComponent<HitFeedback>();

        if (hitFeedback != null)
            hitFeedback.Die(() => Destroy(gameObject));
        else
            Destroy(gameObject);
    }
    
    // private void Die()
    // {
    //     isDead = true;
    //     
    //     OnTowerDestroyed?.Invoke();
    //
    //     var hitFeedback = GetComponent<HitFeedback>();
    //     if (hitFeedback != null)
    //         hitFeedback.Die(() => Destroy(gameObject));
    //     else
    //         Destroy(gameObject);
    // }
}

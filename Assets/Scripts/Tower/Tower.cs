using System;
using UnityEngine;

public class Tower : MonoBehaviour, IDamageable, IBoostable
{
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private bool isDead;
    private bool isShielded;
    private Renderer[] visualRenderers;
    private ProjectileShooter shooter;

    // Captured once in Awake, before any boost is ever applied — what an
    // expired Attack Speed boost reverts back to.
    private Material[][] baseMaterials;
    private Vector3 baseScale;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action OnTowerDestroyed;

    public static event Action<Tower> OnTowerSpawned;

    private void Awake()
    {
        currentHealth = maxHealth;
        visualRenderers = BoostVisuals.FindModelRenderers(gameObject);
        shooter = GetComponent<ProjectileShooter>();

        baseMaterials = BoostVisuals.CaptureBaseMaterials(visualRenderers);
        baseScale = transform.localScale;

        ApplyAlreadyActiveBoosts();

        OnTowerSpawned?.Invoke(this);
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
            shooter?.ApplyStatBonus(definition.damageBonus, 0f, definition.fireIntervalReduction);
            BoostVisuals.ApplyAttackSpeedLook(visualRenderers, transform, definition);
        }

        Debug.Log($"{name} activated {definition.label}.");
    }

    public void RevertBoost(BoostType type)
    {
        if (type == BoostType.Shield)
        {
            isShielded = false;
            BoostVisuals.RemoveShield(transform);
        }
        else if (type == BoostType.AttackSpeed)
        {
            shooter?.ResetToBase();
            BoostVisuals.RevertAttackSpeedLook(visualRenderers, baseMaterials, transform, baseScale);
        }

        Debug.Log($"{name}'s {type} boost ended.");
    }

    public void TakeDamage(float amount)
    {
        if (isDead || isShielded) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        Debug.Log($"Tower took {amount} damage, {currentHealth} / {maxHealth} HP left");
        AudioManager.PlayTowerCrack(transform.position);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        GetComponent<HitFeedback>()?.Flash();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        OnTowerDestroyed?.Invoke();

        var hitFeedback = GetComponent<HitFeedback>();
        if (hitFeedback != null)
            hitFeedback.Die(() => Destroy(gameObject));
        else
            Destroy(gameObject);
    }
}

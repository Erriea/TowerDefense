using UnityEngine;

// DEFENDER BASE CLASS
public abstract class Defender : MonoBehaviour, IDamageable, IBoostable
{
    [SerializeField] protected float maxHealth = 50f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackRange = 5f;
    [SerializeField] protected float attackInterval = 1.5f;

    protected float currentHealth;
    protected float attackTimer;
    private EnemyHealthBar healthBar;
    private Renderer[] visualRenderers;
    private bool isShielded;

    // Captured once in Awake, before any boost is ever applied — what an
    // expired Attack Speed boost reverts back to.
    private float baseAttackDamage;
    private float baseAttackInterval;
    private Material[][] baseMaterials;
    private Vector3 baseScale;

    public abstract DefenderKind Kind { get; }

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        healthBar = GetComponentInChildren<EnemyHealthBar>(true);
        visualRenderers = BoostVisuals.FindModelRenderers(gameObject);

        baseAttackDamage = attackDamage;
        baseAttackInterval = attackInterval;
        baseMaterials = BoostVisuals.CaptureBaseMaterials(visualRenderers);
        baseScale = transform.localScale;

        healthBar?.SetHealth(currentHealth, maxHealth);

        ApplyAlreadyActiveBoosts();
    }

    protected virtual void OnEnable()
    {
        BoostManager.OnBoostActivated += HandleBoostActivated;
        BoostManager.OnBoostEnded += HandleBoostEnded;
    }

    protected virtual void OnDisable()
    {
        BoostManager.OnBoostActivated -= HandleBoostActivated;
        BoostManager.OnBoostEnded -= HandleBoostEnded;
    }

    private void HandleBoostActivated(DefenderKind kind, BoostType type, BoostDefinition definition)
    {
        if (kind == Kind)
            ApplyBoost(type, definition);
    }

    private void HandleBoostEnded(DefenderKind kind, BoostType type)
    {
        if (kind == Kind)
            RevertBoost(type);
    }

    // Picks up whichever of this Kind's two boosts (Shield, Attack Speed) are
    // already running globally — keeps a freshly-spawned defender in sync
    // with whatever's already been bought.
    private void ApplyAlreadyActiveBoosts()
    {
        if (BoostManager.Instance == null) return;

        if (BoostManager.Instance.IsActive(Kind, BoostType.Shield))
            ApplyBoost(BoostType.Shield, BoostManager.Instance.GetDefinition(Kind, BoostType.Shield));

        if (BoostManager.Instance.IsActive(Kind, BoostType.AttackSpeed))
            ApplyBoost(BoostType.AttackSpeed, BoostManager.Instance.GetDefinition(Kind, BoostType.AttackSpeed));
    }

    public virtual void ApplyBoost(BoostType type, BoostDefinition definition)
    {
        if (definition == null) return;

        if (type == BoostType.Shield)
        {
            isShielded = true;
            BoostVisuals.ApplyShield(transform, visualRenderers);
        }
        else if (type == BoostType.AttackSpeed)
        {
            attackDamage += definition.damageBonus;
            attackInterval = Mathf.Max(0.2f, attackInterval - definition.fireIntervalReduction);
            BoostVisuals.ApplyAttackSpeedLook(visualRenderers, transform, definition);
        }

        Debug.Log($"{name} activated {definition.label}.");
    }

    // Called when this Kind's boost timer runs out — undoes exactly that one
    // boost's effect, leaving the other boost (if active) untouched.
    public virtual void RevertBoost(BoostType type)
    {
        if (type == BoostType.Shield)
        {
            isShielded = false;
            BoostVisuals.RemoveShield(transform);
        }
        else if (type == BoostType.AttackSpeed)
        {
            attackDamage = baseAttackDamage;
            attackInterval = baseAttackInterval;
            BoostVisuals.RevertAttackSpeedLook(visualRenderers, baseMaterials, transform, baseScale);
        }

        Debug.Log($"{name}'s {type} boost ended.");
    }

    protected virtual void Update()
    {
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            TryAttack();
        }
    }

    protected virtual void TryAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                AudioManager.PlayAttack(transform.position);
                Debug.Log($"{name} attacked {enemy.name} for {attackDamage} damage");
                attackTimer = 0f;
                return;
            }
        }
    }

    public virtual void TakeDamage(float amount)
    {
        if (isShielded) return; // Shield boost active — no damage taken

        currentHealth -= amount;
        healthBar?.SetHealth(currentHealth, maxHealth);
        GetComponent<HitFeedback>()?.Flash();
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        var hitFeedback = GetComponent<HitFeedback>();
        if (hitFeedback != null)
            hitFeedback.Die(() => Destroy(gameObject));
        else
            Destroy(gameObject);
    }
}

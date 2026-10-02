using System.Collections.Generic;
using UnityEngine;

// ENEMY BASE CLASS
public abstract class Enemy : MonoBehaviour, IDamageable
{
    protected enum State { Walking, Attacking }

    [SerializeField] protected float maxHealth = 20f;
    [SerializeField] protected float moveSpeed = 3f;
    [SerializeField] protected float waypointReachedDistance = 0.2f;
    [SerializeField] protected float damageToTower = 10f;

    [SerializeField] protected float detectionRange = 4f;
    [SerializeField] protected float damageToDefender = 5f;
    [SerializeField] protected float attackInterval = 1f;

    protected float currentHealth;
    private EnemyHealthBar healthBar;
    protected List<Vector3> waypoints;
    protected int currentWaypointIndex;
    protected IDamageable target;

    protected State state = State.Walking;
    protected Defender targetDefender;
    protected float attackTimer;

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public void Initialize(List<Vector3> path, IDamageable target)
    {
        waypoints = path;
        currentWaypointIndex = 0;
        currentHealth = maxHealth;
        this.target = target;

        healthBar = GetComponentInChildren<EnemyHealthBar>(true);
        healthBar?.SetHealth(currentHealth, maxHealth);
    }

    protected virtual void Update()
    {
        if (state == State.Attacking)
        {
            UpdateAttacking();
            return;
        }

        UpdateWalking();
    }

    protected virtual void UpdateWalking()
    {
        Defender nearbyDefender = FindNearbyDefender();

        if (nearbyDefender != null)
        {
            targetDefender = nearbyDefender;
            state = State.Attacking;
            attackTimer = 0f;
            return;
        }

        if (waypoints == null || currentWaypointIndex >= waypoints.Count)
            return;

        Vector3 waypointTarget = waypoints[currentWaypointIndex];
        Vector3 direction = waypointTarget - transform.position;

        transform.position = Vector3.MoveTowards(transform.position, waypointTarget, moveSpeed * Time.deltaTime);

        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direction);

        if (Vector3.Distance(transform.position, waypointTarget) < waypointReachedDistance)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Count)
            {
                ReachTower();
            }
        }
    }

    protected virtual void UpdateAttacking()
    {
        if (targetDefender == null)
        {
            state = State.Walking;
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= attackInterval)
        {
            targetDefender.TakeDamage(damageToDefender);
            AudioManager.PlayAttack(transform.position);
            Debug.Log($"{name} attacked {targetDefender.name}");
            attackTimer = 0f;
        }
    }

    protected virtual Defender FindNearbyDefender()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);

        foreach (var hit in hits)
        {
            Defender defender = hit.GetComponent<Defender>();

            if (defender != null)
            {
                return defender;
            }
        }

        return null;
    }

    protected virtual void ReachTower()
    {
        Debug.Log($"{name} reached the tower!");

        target?.TakeDamage(damageToTower);
        Destroy(gameObject);
    }

    public static event System.Action OnAnyEnemyDespawned;

    private void OnDestroy()
    {
        OnAnyEnemyDespawned?.Invoke();
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{name} took {amount} damage, {currentHealth} HP left");

        healthBar?.SetHealth(currentHealth, maxHealth);
        GetComponent<HitFeedback>()?.Flash();

        if (currentHealth <= 0)
        {
            var hitFeedback = GetComponent<HitFeedback>();
            if (hitFeedback != null)
                hitFeedback.Die(() => Destroy(gameObject));
            else
                Destroy(gameObject);
        }
    }
}
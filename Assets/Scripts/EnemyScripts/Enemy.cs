using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private enum State { Walking, Attacking }

    [SerializeField] private float maxHealth = 20f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float waypointReachedDistance = 0.2f;
    [SerializeField] private float damageToTower = 10f;

    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private float damageToDefender = 5f;
    [SerializeField] private float attackInterval = 1f;

    private float currentHealth;
    private List<Vector3> waypoints;
    private int currentWaypointIndex;
    private IDamageable target;

    private State state = State.Walking;
    private Defender targetDefender;
    private float attackTimer;

    public void Initialize(List<Vector3> path, IDamageable target)
    {
        waypoints = path;
        currentWaypointIndex = 0;
        currentHealth = maxHealth;
        this.target = target;
    }

    private void Update()
    {
        if (state == State.Attacking)
        {
            UpdateAttacking();
            return;
        }

        UpdateWalking();
    }

    private void UpdateWalking()
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

    private void UpdateAttacking()
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
            Debug.Log($"{name} attacked {targetDefender.name}");
            attackTimer = 0f;
        }
    }

    private Defender FindNearbyDefender()
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

    private void ReachTower()
    {
        Debug.Log("Crow reached the tower!");

        target?.TakeDamage(damageToTower);
        Destroy(gameObject);
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"{name} took {amount} damage, {currentHealth} HP left");

        if (currentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
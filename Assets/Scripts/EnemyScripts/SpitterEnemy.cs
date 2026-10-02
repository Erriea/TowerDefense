using UnityEngine;

// SpitterEnemy.cs — sneaks toward its target, stops and switches to Idle/Throw
// once something's in range, and lets its ProjectileShooter do the attacking
public class SpitterEnemy : Enemy
{
    [SerializeField] private float rangedAttackDistance = 10f;
    [SerializeField] private Animator animator;

    protected override void UpdateWalking()
    {
        Defender nearbyDefender = FindNearbyDefender();

        if (nearbyDefender != null)
        {
            EnterAttacking();
            return;
        }

        if (waypoints == null || currentWaypointIndex >= waypoints.Count)
            return;

        Vector3 finalWaypoint = waypoints[waypoints.Count - 1];

        if (Vector3.Distance(transform.position, finalWaypoint) <= rangedAttackDistance)
        {
            EnterAttacking();
            return;
        }

        Vector3 waypointTarget = waypoints[currentWaypointIndex];
        Vector3 direction = waypointTarget - transform.position;

        transform.position = Vector3.MoveTowards(transform.position, waypointTarget, moveSpeed * Time.deltaTime);

        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direction);

        if (Vector3.Distance(transform.position, waypointTarget) < waypointReachedDistance)
        {
            currentWaypointIndex++;
        }
    }

    protected override void UpdateAttacking()
    {
        bool nearEndOfPath = waypoints != null && waypoints.Count > 0
            && Vector3.Distance(transform.position, waypoints[waypoints.Count - 1]) <= rangedAttackDistance;

        if (FindNearbyDefender() == null && !nearEndOfPath)
        {
            state = State.Walking;
            animator?.SetBool("InRange", false);
        }
    }

    private void EnterAttacking()
    {
        state = State.Attacking;
        attackTimer = 0f;
        animator?.SetBool("InRange", true);
    }
}

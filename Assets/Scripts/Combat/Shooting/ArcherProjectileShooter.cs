using UnityEngine;

// ARCHER PROJECTILE SHOOTER — prioritizes the nearest BomberEnemy in range (her main
// job is shooting bombers down before they reach the tower); if no Bomber is in range,
// falls back to the nearest enemy of any type. Plays the Shoot animation each time she
// fires, and turns to face whatever she's currently targeting (same pattern as Golem/Spitter).
public class ArcherProjectileShooter : ProjectileShooter
{
    [SerializeField] private Animator animator;
    [SerializeField] private float turnSpeed = 180f; // degrees per second

    private Transform currentTarget;

    protected override void Update()
    {
        base.Update();
        FaceCurrentTarget();
    }

    protected override IDamageable FindTarget(out Transform targetTransform)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);

        Enemy nearestBomber = null;
        Transform nearestBomberTransform = null;
        float nearestBomberDistance = float.MaxValue;

        Enemy nearestAny = null;
        Transform nearestAnyTransform = null;
        float nearestAnyDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);

            if (distance < nearestAnyDistance)
            {
                nearestAnyDistance = distance;
                nearestAny = enemy;
                nearestAnyTransform = hit.transform;
            }

            if (enemy is BomberEnemy && distance < nearestBomberDistance)
            {
                nearestBomberDistance = distance;
                nearestBomber = enemy;
                nearestBomberTransform = hit.transform;
            }
        }

        // Bombers always take priority over any other enemy in range.
        if (nearestBomber != null)
        {
            targetTransform = nearestBomberTransform;
            return nearestBomber;
        }

        targetTransform = nearestAnyTransform;
        return nearestAny;
    }

    protected override void FireAt(Transform targetTransform, IDamageable targetDamageable)
    {
        base.FireAt(targetTransform, targetDamageable);
        currentTarget = targetTransform;
        animator?.SetTrigger("Shoot");
    }

    private void FaceCurrentTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0f; // keep the archer upright, only turn left/right

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
}

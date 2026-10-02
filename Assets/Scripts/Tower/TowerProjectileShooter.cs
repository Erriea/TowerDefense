using UnityEngine;

// TOWER PROJECTILE SHOOTER — finds the nearest Enemy in range and fires at it.
public class TowerProjectileShooter : ProjectileShooter
{
    protected override IDamageable FindTarget(out Transform targetTransform)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);

        Enemy nearestEnemy = null;
        Transform nearestTransform = null;
        float nearestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
                nearestTransform = hit.transform;
            }
        }

        targetTransform = nearestTransform;
        return nearestEnemy;
    }
}

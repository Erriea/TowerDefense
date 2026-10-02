using UnityEngine;

// PROJECTILE SHOOTER — shared base for anything that fires projectiles at
// set intervals (Tower, Archer Defender, Spitter). Subclasses only decide
// *who* counts as a valid target; timing, firing and the projectile itself
// are handled here.
public abstract class ProjectileShooter : MonoBehaviour
{
    [SerializeField] protected GameObject projectilePrefab;
    [SerializeField] protected Transform firePoint;
    [SerializeField] protected float range = 8f;
    [SerializeField] protected float fireInterval = 1f;
    [SerializeField] protected float damage = 10f;

    private float fireTimer;

    protected virtual void Update()
    {
        fireTimer += Time.deltaTime;

        if (fireTimer >= fireInterval)
        {
            TryFire();
        }
    }

    protected virtual void TryFire()
    {
        IDamageable target = FindTarget(out Transform targetTransform);

        if (target == null) return;

        FireAt(targetTransform, target);
        fireTimer = 0f;
    }

    protected virtual void FireAt(Transform targetTransform, IDamageable targetDamageable)
    {
        if (projectilePrefab == null) return;

        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;
        GameObject projectileObject = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Projectile projectile = projectileObject.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Launch(targetTransform, targetDamageable, damage);
        }
    }

    // Subclasses decide what counts as a target: Enemy for Tower/Archer,
    // Defender/Tower for Spitter.
    protected abstract IDamageable FindTarget(out Transform targetTransform);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
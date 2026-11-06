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

    private float baseDamage;
    private float baseRange;
    private float baseFireInterval;

    protected virtual void Awake()
    {
        baseDamage = damage;
        baseRange = range;
        baseFireInterval = fireInterval;
    }

    // Used by the upgrade system — Archer/Tower's real combat stats live here,
    // not on Defender's own fields, so upgrades are forwarded to this method.
    public void ApplyStatBonus(float damageBonus, float rangeBonus, float fireIntervalReduction)
    {
        damage += damageBonus;
        range += rangeBonus;
        fireInterval = Mathf.Max(0.2f, fireInterval - fireIntervalReduction);
    }

    // Used when a timed upgrade expires — snaps back to the exact pre-upgrade
    // values rather than trying to subtract bonuses back out (which could drift
    // if fireInterval ever hit its floor clamp above).
    public void ResetToBase()
    {
        damage = baseDamage;
        range = baseRange;
        fireInterval = baseFireInterval;
    }

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
        AudioManager.PlayShot(spawnPosition);
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

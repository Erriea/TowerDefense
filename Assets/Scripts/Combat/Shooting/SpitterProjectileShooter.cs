using UnityEngine;

// SPITTER PROJECTILE SHOOTER — finds the nearest Defender or the Tower in
// range and fires at it, playing the Throw animation each time it does
public class SpitterProjectileShooter : ProjectileShooter
{
    [SerializeField] private Animator animator;

    protected override IDamageable FindTarget(out Transform targetTransform)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, range);

        IDamageable nearestTarget = null;
        Transform nearestTransform = null;
        float nearestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            IDamageable damageable = hit.GetComponent<Defender>() as IDamageable;
            if (damageable == null)
                damageable = hit.GetComponent<Tower>() as IDamageable;

            if (damageable == null) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = damageable;
                nearestTransform = hit.transform;
            }
        }

        targetTransform = nearestTransform;
        return nearestTarget;
    }

    protected override void FireAt(Transform targetTransform, IDamageable targetDamageable)
    {
        base.FireAt(targetTransform, targetDamageable);
        animator?.SetTrigger("Throw");
    }
}

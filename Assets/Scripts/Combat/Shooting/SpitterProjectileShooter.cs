using UnityEngine;

// SPITTER PROJECTILE SHOOTER — finds the nearest Defender or the Tower in
// range and fires at it, playing the Throw animation each time it does, and
// turning to face whatever it's currently targeting (same pattern as Golem).
public class SpitterProjectileShooter : ProjectileShooter
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
        currentTarget = targetTransform;
        animator?.SetTrigger("Throw");
    }

    private void FaceCurrentTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0f; // keep the spitter upright, only turn left/right

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
}

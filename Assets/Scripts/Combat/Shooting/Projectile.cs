using UnityEngine;

// PROJECTILE — a generic sphere shot that flies toward a target and deals
// damage on arrival. Works for any shooter (Tower, Archer, Spitter later)
// since it only knows about IDamageable, not who fired it.
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float hitDistance = 0.3f;
    [SerializeField] private float lifeTime = 5f;

    private Transform targetTransform;
    private IDamageable targetDamageable;
    private float damage;
    private Vector3 lastKnownPosition;

    // Set by an active Wind environmental event — a fixed lateral offset blended into every
    // in-flight projectile's aim, battlefield-wide. Static (not per-instance) because wind is
    // a condition of the whole scene, not something each shot has its own copy of. Since it's
    // a constant offset rather than one scaled by distance, it becomes proportionally more
    // significant as a shot closes in, which is what actually produces a visible last-moment
    // curve/miss instead of the homing correction just cancelling it out every frame.
    private static Vector3 windDrift = Vector3.zero;

    public static void SetWind(Vector3 drift)
    {
        windDrift = drift;
    }

    public void Launch(Transform targetTransform, IDamageable targetDamageable, float damage)
    {
        this.targetTransform = targetTransform;
        this.targetDamageable = targetDamageable;
        this.damage = damage;
        lastKnownPosition = targetTransform != null ? targetTransform.position : transform.position;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        Vector3 destination = targetTransform != null ? targetTransform.position : lastKnownPosition;
        lastKnownPosition = destination;

        Vector3 toTarget = destination - transform.position;
        float step = speed * Time.deltaTime;

        // Arrival is judged on the true, un-drifted distance — wind should make a shot curve
        // and occasionally miss its mark, not change what "arrived" means.
        if (toTarget.magnitude <= Mathf.Max(step, hitDistance))
        {
            Hit();
            return;
        }

        Vector3 moveDirection = (toTarget + windDrift).normalized;
        transform.position += moveDirection * step;
        transform.rotation = Quaternion.LookRotation(moveDirection);
    }

    private void Hit()
    {
        // targetTransform will be null if the target died mid-flight — in
        // that case the projectile just fizzles out instead of damaging nothing.
        if (targetTransform != null)
        {
            targetDamageable?.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
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

        Vector3 direction = destination - transform.position;
        float step = speed * Time.deltaTime;

        if (direction.magnitude <= Mathf.Max(step, hitDistance))
        {
            Hit();
            return;
        }

        transform.position += direction.normalized * step;
        transform.rotation = Quaternion.LookRotation(direction);
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
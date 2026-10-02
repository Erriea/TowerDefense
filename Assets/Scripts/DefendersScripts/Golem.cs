using UnityEngine;

// Golem — attacks the nearest enemy in range, plays its Attack animation,
// flashes a ring showing its attack radius, and turns to face whatever it's attacking
public class Golem : Defender
{
    [SerializeField] private Animator animator;
    [SerializeField] private AttackRadiusEffect radiusEffect;
    [SerializeField] private float turnSpeed = 180f; // degrees per second

    private Transform currentTarget;

    protected override void Update()
    {
        base.Update();
        FaceCurrentTarget();
    }

    protected override void TryAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

        foreach (var hit in hits)
        {
            Enemy enemy = hit.GetComponent<Enemy>();

            if (enemy != null)
            {
                currentTarget = hit.transform;

                enemy.TakeDamage(attackDamage);
                animator?.SetTrigger("Attack");
                radiusEffect?.Play(attackRange);
                Debug.Log($"{name} attacked {enemy.name} for {attackDamage} damage");
                attackTimer = 0f;
                return;
            }
        }
    }

    private void FaceCurrentTarget()
    {
        if (currentTarget == null) return;

        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0f; // keep the golem upright, only turn left/right

        if (direction.sqrMagnitude < 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }
}
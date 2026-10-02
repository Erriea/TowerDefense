using UnityEngine;

// ArcherDefender.cs — long attack range, low health, and actively targets the nearest enemy in range
public class ArcherDefender : Defender
{
    protected override void TryAttack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange);

        Enemy nearestEnemy = null;
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
            }
        }

        if (nearestEnemy != null)
        {
            nearestEnemy.TakeDamage(attackDamage);
            Debug.Log($"{name} fired an arrow at {nearestEnemy.name} for {attackDamage} damage");
            attackTimer = 0f;
        }
    }
}
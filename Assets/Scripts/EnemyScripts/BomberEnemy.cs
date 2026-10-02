using UnityEngine;

// BomberEnemy.cs — ignores the generated path entirely. On its first update it locks onto
// whichever is nearer right now — a Defender or the tower — rises to a fixed flying height,
// and flies a dead-straight line to that one target. No retargeting mid-flight, no pathing.
public class BomberEnemy : Enemy
{
    [SerializeField] private float flyHeight = 6f;
    [SerializeField] private float huntRange = 15f;
    [SerializeField] private float detonateDistance = 1f;
    [SerializeField] private float explosionDamage = 40f;
    [SerializeField] private GameObject explosionEffectPrefab;

    private bool hasLockedOn;
    private Defender lockedDefender;

    protected override void UpdateWalking()
    {
        if (!hasLockedOn)
        {
            LockOnNearestTarget();
        }

        Vector3 chaseTarget = CurrentTargetPosition();

        if (Vector3.Distance(transform.position, chaseTarget) <= detonateDistance)
        {
            Detonate();
            return;
        }

        Vector3 direction = chaseTarget - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, chaseTarget, moveSpeed * Time.deltaTime);

        if (direction.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(direction);
    }

    // Picks the target ONCE and rises to flying height. Everything after this is a straight line.
    private void LockOnNearestTarget()
    {
        transform.position = new Vector3(transform.position.x, TowerPosition().y + flyHeight, transform.position.z);

        Defender nearestDefender = FindNearestDefender();
        Vector3 towerPos = TowerPosition();

        if (nearestDefender != null &&
            Vector3.Distance(transform.position, nearestDefender.transform.position) < Vector3.Distance(transform.position, towerPos))
        {
            lockedDefender = nearestDefender;
        }

        hasLockedOn = true;
    }

    // If the locked defender dies before the bomber arrives, this naturally falls back to the
    // tower next frame (Unity treats a destroyed object reference as null) instead of crashing.
    private Vector3 CurrentTargetPosition()
    {
        if (lockedDefender != null)
            return new Vector3(lockedDefender.transform.position.x, transform.position.y, lockedDefender.transform.position.z);

        Vector3 towerPos = TowerPosition();
        return new Vector3(towerPos.x, transform.position.y, towerPos.z);
    }

    // The tower's real position, not the path's final waypoint — Bomber doesn't use the path.
    private Vector3 TowerPosition()
    {
        if (target is Tower tower)
            return tower.transform.position;

        return waypoints != null && waypoints.Count > 0 ? waypoints[waypoints.Count - 1] : transform.position;
    }

    private Defender FindNearestDefender()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, huntRange);
        Defender nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            Defender defender = hit.GetComponent<Defender>();
            if (defender == null) continue;

            float distance = Vector3.Distance(transform.position, defender.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = defender;
            }
        }

        return nearest;
    }

    private void Detonate()
    {
        Debug.Log($"{name} detonated for {explosionDamage} damage");

        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        if (lockedDefender != null)
            lockedDefender.TakeDamage(explosionDamage);
        else
            target?.TakeDamage(explosionDamage);

        var hitFeedback = GetComponent<HitFeedback>();
        if (hitFeedback != null)
            hitFeedback.Die(() => Destroy(gameObject));
        else
            Destroy(gameObject);
    }
}

using System.Collections.Generic;
using UnityEngine;

// BOMBER ENEMY — ignores the generated path entirely. On its first update it locks onto
// whichever is nearer right now — a Defender or the tower — rises to a fixed flying height,
// and flies toward that one target. No retargeting mid-flight, no pathing.
//
// FLOCKING (boids) — crow-to-crow only: on top of the pull toward its locked target, each
// Bomber also steers using the three classic boid rules against OTHER BOMBERS ONLY —
// separation (don't overlap nearby crows), alignment (match their heading), cohesion (drift
// toward the local group's center). The target-seek pull always dominates the blend, so a
// Bomber never stops being a working enemy — it just visibly clumps and weaves as a flock on
// the way in. Each Bomber's rule weights are jittered at spawn, so no two flocks ever move
// identically even with the same enemy count and the same target.
public class BomberEnemy : Enemy
{
    [SerializeField] private float flyHeight = 6f;
    [SerializeField] private float huntRange = 15f;
    [SerializeField] private float detonateDistance = 1f;
    [SerializeField] private float explosionDamage = 40f;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("Flocking (Boids) — crow-to-crow only")]
    [SerializeField] private float flockRadius = 20f;   // must comfortably cover the cluster spawn spread, or crows spawn too far apart to ever see each other as flockmates
    [SerializeField] private float separationRadius = 9f;   // bigger = more personal space; this sets the flock's natural resting gap
    [SerializeField] private float targetSeekWeight = 2.0f;
    [SerializeField] private float separationWeight = 2.2f;
    [SerializeField] private float alignmentWeight = 1.0f;
    [SerializeField] private float cohesionWeight = 0.5f;
    [SerializeField, Range(0f, 1f)] private float weightJitter = 0.2f;

    [Header("Wander — small persistent per-crow meander, so flights never look ruler-straight")]
    [SerializeField] private float wanderWeight = 1.1f;
    [SerializeField] private float wanderNoiseScale = 0.35f;  // how quickly the wander direction drifts over time

    private bool hasLockedOn;
    private Defender lockedDefender;

    // This crow's current facing direction — read by nearby crows for the alignment rule.
    private Vector3 currentHeading = Vector3.forward;

    // Jittered once at spawn (±weightJitter) so every crow has a slightly different
    // "personality" — the actual procedurally-generated part of this feature.
    private float myTargetSeekWeight;
    private float mySeparationWeight;
    private float myAlignmentWeight;
    private float myCohesionWeight;
    private float myWanderWeight;

    // Unique per crow so Perlin-noise wander never lines up between flockmates.
    private float wanderSeedX;
    private float wanderSeedZ;

    private void Awake()
    {
        myTargetSeekWeight = Jittered(targetSeekWeight);
        mySeparationWeight = Jittered(separationWeight);
        myAlignmentWeight = Jittered(alignmentWeight);
        myCohesionWeight = Jittered(cohesionWeight);
        myWanderWeight = Jittered(wanderWeight);

        wanderSeedX = Random.Range(0f, 1000f);
        wanderSeedZ = Random.Range(0f, 1000f);
    }

    private float Jittered(float baseValue)
    {
        return baseValue * Random.Range(1f - weightJitter, 1f + weightJitter);
    }

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

        Vector3 seekDirection = SafeDirection(chaseTarget - transform.position);

        List<BomberEnemy> flockmates = FindNearbyCrows();
        Vector3 separationDirection = ComputeSeparation(flockmates);
        Vector3 alignmentDirection = ComputeAlignment(flockmates);
        Vector3 cohesionDirection = ComputeCohesion(flockmates);
        Vector3 wanderDirection = ComputeWander();

        Vector3 blended =
            myTargetSeekWeight * seekDirection +
            mySeparationWeight * separationDirection +
            myAlignmentWeight * alignmentDirection +
            myCohesionWeight * cohesionDirection +
            myWanderWeight * wanderDirection;

        Vector3 finalDirection = SafeDirection(blended, seekDirection);
        currentHeading = finalDirection;

        transform.position += finalDirection * moveSpeed * Time.deltaTime;

        if (finalDirection.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(finalDirection);
    }

    // Other Bombers within flockRadius — crow-to-crow only, no other enemy type joins the flock.
    private List<BomberEnemy> FindNearbyCrows()
    {
        List<BomberEnemy> flockmates = new List<BomberEnemy>();
        Collider[] hits = Physics.OverlapSphere(transform.position, flockRadius);

        foreach (var hit in hits)
        {
            BomberEnemy other = hit.GetComponent<BomberEnemy>();
            if (other != null && other != this)
                flockmates.Add(other);
        }

        return flockmates;
    }

    // Steers away from flockmates closer than separationRadius — stronger the closer they are,
    // so crows never visibly stack on top of each other.
    private Vector3 ComputeSeparation(List<BomberEnemy> flockmates)
    {
        Vector3 push = Vector3.zero;

        foreach (BomberEnemy other in flockmates)
        {
            Vector3 offset = transform.position - other.transform.position;
            float distance = offset.magnitude;

            if (distance > 0.001f && distance < separationRadius)
                push += offset.normalized / distance;
        }

        return SafeDirection(push);
    }

    // Matches the average heading of nearby crows.
    private Vector3 ComputeAlignment(List<BomberEnemy> flockmates)
    {
        if (flockmates.Count == 0) return Vector3.zero;

        Vector3 averageHeading = Vector3.zero;
        foreach (BomberEnemy other in flockmates)
            averageHeading += other.currentHeading;

        return SafeDirection(averageHeading / flockmates.Count);
    }

    // Gently pulls toward the center of the local group, so crows don't scatter.
    private Vector3 ComputeCohesion(List<BomberEnemy> flockmates)
    {
        if (flockmates.Count == 0) return Vector3.zero;

        Vector3 averagePosition = Vector3.zero;
        foreach (BomberEnemy other in flockmates)
            averagePosition += other.transform.position;

        averagePosition /= flockmates.Count;

        return SafeDirection(averagePosition - transform.position);
    }

    // A smoothly-drifting random direction, unique per crow (via its own noise seed) and
    // continuous over time (via Perlin noise, not pure Random) so it meanders instead of
    // jittering frame to frame. This is what keeps a flight path from looking ruler-straight.
    private Vector3 ComputeWander()
    {
        float t = Time.time * wanderNoiseScale;
        float noiseX = Mathf.PerlinNoise(t + wanderSeedX, 0f) * 2f - 1f;
        float noiseZ = Mathf.PerlinNoise(0f, t + wanderSeedZ) * 2f - 1f;
        return SafeDirection(new Vector3(noiseX, 0f, noiseZ));
    }

    // Normalizes v, or returns fallback (default: zero) if v is too small to have a direction.
    private Vector3 SafeDirection(Vector3 v, Vector3 fallback = default)
    {
        return v.sqrMagnitude > 0.0001f ? v.normalized : fallback;
    }

    // Picks the target ONCE and rises to flying height. Everything after this is steered by
    // the target-seek + flocking blend above, rather than a single straight line.
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
        // Convert the IDamageable target into a Tower
        Tower tower = target as Tower;

        // Check whether the Tower still exists
        if (tower != null)
        {
            return tower.transform.position;
        }

        // If the target was destroyed, search for a new Tower
        tower = FindFirstObjectByType<Tower>();

        if (tower != null)
        {
            target = tower;
            return tower.transform.position;
        }

        // Fallback to the final waypoint
        if (waypoints != null && waypoints.Count > 0)
        {
            return waypoints[waypoints.Count - 1];
        }

        // No Tower or waypoints available
        return transform.position;
    }
    // private Vector3 TowerPosition()
    // {
    //     if (target is Tower tower)
    //
    //         return tower.transform.position;
    //
    //     // Fallback for testing: if this Bomber was dropped straight into the scene instead of
    //     // spawned via MonsterSpawner, Initialize() never ran, so target/waypoints are both null.
    //     // Find the real Tower and wire it up as the target so TakeDamage calls actually land,
    //     // instead of silently no-op'ing and detonating on this object's own position.
    //     Tower sceneTower = FindAnyObjectByType<Tower>();
    //     if (sceneTower != null)
    //     {
    //         target = sceneTower;
    //         return sceneTower.transform.position;
    //     }
    //
    //     return waypoints != null && waypoints.Count > 0 ? waypoints[waypoints.Count - 1] : transform.position;
    // }

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
        AudioManager.PlayExplosion(transform.position);

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

using UnityEngine;

// ArcherDefender.cs — places and takes damage like any Defender, but doesn't attack
// on its own. ArcherProjectileShooter (a separate component on the same prefab)
// handles targeting, firing and facing — same split used for the Spitter.
public class ArcherDefender : Defender
{
    protected override void TryAttack()
    {
        // No melee attack here — ArcherProjectileShooter does the actual attacking.
    }
}

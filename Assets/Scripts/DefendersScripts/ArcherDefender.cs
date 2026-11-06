using UnityEngine;

// ArcherDefender.cs — places and takes damage like any Defender, but doesn't attack
// on its own. ArcherProjectileShooter (a separate component on the same prefab)
// handles targeting, firing and facing — same split used for the Spitter.
public class ArcherDefender : Defender
{
    public override DefenderKind Kind => DefenderKind.Sniper; // displayed as "Sniper"

    protected override void TryAttack()
    {
        // No melee attack here — ArcherProjectileShooter does the actual attacking.
    }

    public override void ApplyBoost(BoostType type, BoostDefinition definition)
    {
        base.ApplyBoost(type, definition); // Shield flag / visual, or health+visual for Attack Speed

        // Sniper's real attack stats live on its sibling ProjectileShooter, not
        // on Defender's own fields, so forward the Attack Speed effect there too.
        if (type == BoostType.AttackSpeed)
            GetComponent<ProjectileShooter>()?.ApplyStatBonus(definition.damageBonus, 0f, definition.fireIntervalReduction);
    }

    public override void RevertBoost(BoostType type)
    {
        base.RevertBoost(type);

        // Same split as ApplyBoost — the sibling shooter needs its own reset too.
        if (type == BoostType.AttackSpeed)
            GetComponent<ProjectileShooter>()?.ResetToBase();
    }
}

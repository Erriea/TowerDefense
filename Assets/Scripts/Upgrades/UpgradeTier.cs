using UnityEngine;

// UpgradeTier — a single purchasable upgrade step for a defender type or the tower.
// Configured per type on the UpgradeManager; applied additively on top of base stats.
[System.Serializable]
public class UpgradeTier
{
    public string label = "Upgrade";
    public float cost = 5f;

    [Header("Stat bonuses (added to current stats)")]
    public float healthBonus = 20f;
    public float damageBonus = 0f;
    public float rangeBonus = 0f;
    public float attackIntervalReduction = 0f;

    [Header("Visual change")]
    public Material overrideMaterial;
    public float scaleMultiplier = 1.1f;
    public bool pulses = false; // dramatic pulsing emission instead of a steady glow
}

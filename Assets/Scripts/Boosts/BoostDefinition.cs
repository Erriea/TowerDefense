using UnityEngine;

// BoostDefinition — one (DefenderKind, BoostType) boost's cost, duration and
// effect. Shield boosts only use label/cost/duration (the effect is just
// "take no damage", handled directly in Defender/Tower). AttackSpeed boosts
// also use the stat and visual fields below.
[System.Serializable]
public class BoostDefinition
{
    public string label = "Boost";
    public float cost = 5f;
    public float duration = 10f;

    [Header("Attack Speed effect (unused by Shield)")]
    public float damageBonus = 0f;
    public float fireIntervalReduction = 0f;

    [Header("Attack Speed visual (unused by Shield)")]
    public Material overrideMaterial;
    public float scaleMultiplier = 1f;
    public bool pulses = false;
}

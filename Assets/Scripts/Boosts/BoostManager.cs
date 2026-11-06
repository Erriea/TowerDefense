using System;
using System.Collections;
using UnityEngine;

// BOOST MANAGER — owns every (DefenderKind, BoostType) boost's definition and
// live timer. Buying a boost costs mana, runs for its own duration, and is
// completely independent of every other (kind, type) combination — buying the
// Tower's Shield never touches the Tower's own Attack Speed boost, or any
// other kind's boosts. While a boost is active it can't be bought again; it
// has to fully expire first. Mirrors AudioManager's static-singleton shape.
public class BoostManager : MonoBehaviour
{
    private const int KindCount = 4; // Golem, Sniper, Raccoon, Tower
    private const int TypeCount = 2; // Shield, AttackSpeed

    public static BoostManager Instance { get; private set; }

    [SerializeField] private Mana manaSystem;

    private readonly BoostDefinition[] definitions = new BoostDefinition[KindCount * TypeCount];
    private readonly float[] expireAt = new float[KindCount * TypeCount];
    private readonly Coroutine[] revertRoutines = new Coroutine[KindCount * TypeCount];

    public static event Action<DefenderKind, BoostType, BoostDefinition> OnBoostActivated;
    public static event Action<DefenderKind, BoostType> OnBoostEnded;

    private static int Index(DefenderKind kind, BoostType type) => (int)kind * TypeCount + (int)type;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        for (int i = 0; i < expireAt.Length; i++) expireAt[i] = -1f;

        EnsureDefinitions();
    }

    // Guarantees every (kind, type) pair has a usable definition even if none
    // were configured in the Inspector, so the feature works with no setup.
    private void EnsureDefinitions()
    {
        // Attack Speed reuses each type's existing "boosted" look (the old
        // Tier II glow/growth/pulse materials) — see Assets/GameAssets/Materials/Resources/.
        Material golemMaterial = Resources.Load<Material>("GolemTier2");
        Material sniperMaterial = Resources.Load<Material>("SniperTier2");
        Material raccoonMaterial = Resources.Load<Material>("RaccoonTier2");
        Material towerMaterial = Resources.Load<Material>("TowerTier2");

        Set(DefenderKind.Golem, BoostType.Shield, new BoostDefinition
        {
            label = "Golem Shield", cost = 5f, duration = 10f
        });
        Set(DefenderKind.Golem, BoostType.AttackSpeed, new BoostDefinition
        {
            label = "Golem Speed", cost = 5f, duration = 10f,
            damageBonus = 8f, fireIntervalReduction = 0.5f,
            overrideMaterial = golemMaterial, scaleMultiplier = 1.35f
        });

        Set(DefenderKind.Sniper, BoostType.Shield, new BoostDefinition
        {
            label = "Sniper Shield", cost = 6f, duration = 10f
        });
        Set(DefenderKind.Sniper, BoostType.AttackSpeed, new BoostDefinition
        {
            label = "Sniper Speed", cost = 6f, duration = 10f,
            damageBonus = 6f, fireIntervalReduction = 0.6f,
            overrideMaterial = sniperMaterial, scaleMultiplier = 1.4f
        });

        Set(DefenderKind.Raccoon, BoostType.Shield, new BoostDefinition
        {
            label = "Raccoon Shield", cost = 4f, duration = 10f
        });
        Set(DefenderKind.Raccoon, BoostType.AttackSpeed, new BoostDefinition
        {
            label = "Raccoon Speed", cost = 4f, duration = 10f,
            damageBonus = 4f, fireIntervalReduction = 0.4f,
            overrideMaterial = raccoonMaterial, scaleMultiplier = 1.75f, pulses = true
        });

        Set(DefenderKind.Tower, BoostType.Shield, new BoostDefinition
        {
            label = "Tower Shield", cost = 5f, duration = 10f
        });
        Set(DefenderKind.Tower, BoostType.AttackSpeed, new BoostDefinition
        {
            label = "Tower Speed", cost = 5f, duration = 10f,
            damageBonus = 10f, fireIntervalReduction = 0.5f,
            overrideMaterial = towerMaterial, scaleMultiplier = 1.1f
        });
    }

    private void Set(DefenderKind kind, BoostType type, BoostDefinition definition)
    {
        definitions[Index(kind, type)] = definition;
    }

    public BoostDefinition GetDefinition(DefenderKind kind, BoostType type) => definitions[Index(kind, type)];

    // Seconds left on this (kind, type) boost, or 0 if it isn't active —
    // what the boost button polls to draw its cooldown countdown.
    public float GetRemainingSeconds(DefenderKind kind, BoostType type)
    {
        float e = expireAt[Index(kind, type)];
        return e > 0f ? Mathf.Max(0f, e - Time.time) : 0f;
    }

    public bool IsActive(DefenderKind kind, BoostType type) => GetRemainingSeconds(kind, type) > 0f;

    public bool TryPurchaseBoost(DefenderKind kind, BoostType type)
    {
        int index = Index(kind, type);

        if (IsActive(kind, type))
        {
            Debug.Log($"{kind}'s {type} boost is already active.");
            return false;
        }

        BoostDefinition definition = definitions[index];
        if (definition == null) return false;

        if (manaSystem == null || !manaSystem.TryUseMana(definition.cost))
        {
            Debug.Log($"Not enough mana for {kind} {type}.");
            return false;
        }

        expireAt[index] = Time.time + definition.duration;
        revertRoutines[index] = StartCoroutine(RevertAfterDelay(kind, type, definition.duration));

        AudioManager.PlayButtonClick();
        Debug.Log($"{kind} activated {definition.label} for {definition.duration}s.");
        OnBoostActivated?.Invoke(kind, type, definition);
        return true;
    }

    private IEnumerator RevertAfterDelay(DefenderKind kind, BoostType type, float delay)
    {
        yield return new WaitForSeconds(delay);

        int index = Index(kind, type);
        expireAt[index] = -1f;
        revertRoutines[index] = null;

        Debug.Log($"{kind}'s {type} boost ended.");
        OnBoostEnded?.Invoke(kind, type);
    }
}

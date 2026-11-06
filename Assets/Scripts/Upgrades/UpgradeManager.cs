using System;
using System.Collections;
using UnityEngine;

// UPGRADE MANAGER — tracks the current global upgrade level per defender type (and the
// Tower) and applies purchases to every current AND future instance of that type.
// Existing instances subscribe to OnUpgradePurchased; new instances read the current
// level in Awake. Mirrors AudioManager's static-singleton shape.
//
// Upgrades are TIMED: buying one starts (or restarts) a flat countdown for that
// type. When it runs out, OnUpgradeReverted fires and every instance of that type
// snaps back to its base stats and look. Buying another tier of the same type
// before the timer ends just restarts the clock — the whole stacked effect still
// expires together.
public class UpgradeManager : MonoBehaviour
{
    private const float UpgradeDurationSeconds = 15f;

    public static UpgradeManager Instance { get; private set; }

    [SerializeField] private Mana manaSystem;

    [SerializeField] private UpgradeTier[] golemTiers;
    [SerializeField] private UpgradeTier[] sniperTiers;
    [SerializeField] private UpgradeTier[] raccoonTiers;
    [SerializeField] private UpgradeTier[] towerTiers;

    private int golemLevel;
    private int sniperLevel;
    private int raccoonLevel;
    private int towerLevel;

    // Time.time each kind's current upgrade expires at; <= 0 means "not active".
    private float golemExpireAt = -1f;
    private float sniperExpireAt = -1f;
    private float raccoonExpireAt = -1f;
    private float towerExpireAt = -1f;

    private Coroutine golemRevertRoutine;
    private Coroutine sniperRevertRoutine;
    private Coroutine raccoonRevertRoutine;
    private Coroutine towerRevertRoutine;

    public static event Action<DefenderKind, UpgradeTier> OnUpgradePurchased;
    public static event Action<DefenderKind> OnUpgradeReverted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        EnsureDefaultTiers();
    }

    // Guarantees every type has two usable tiers even if the Inspector arrays
    // were left empty, so the feature works without needing manual setup first.
    private void EnsureDefaultTiers()
    {
        // Each defender type (and the Tower) gets its own two-tier glow set —
        // real material assets under Assets/GameAssets/Materials/Resources/,
        // swapped onto the model on purchase.
        Material golemTier1 = Resources.Load<Material>("GolemTier1");
        Material golemTier2 = Resources.Load<Material>("GolemTier2");
        Material sniperTier1 = Resources.Load<Material>("SniperTier1");
        Material sniperTier2 = Resources.Load<Material>("SniperTier2");
        Material raccoonTier1 = Resources.Load<Material>("RaccoonTier1");
        Material raccoonTier2 = Resources.Load<Material>("RaccoonTier2");
        Material towerTier1 = Resources.Load<Material>("TowerTier1");
        Material towerTier2 = Resources.Load<Material>("TowerTier2");

        if (golemTiers == null || golemTiers.Length == 0)
        {
            golemTiers = new[]
            {
                new UpgradeTier { label = "Golem I", cost = 4f, healthBonus = 25f, damageBonus = 5f, rangeBonus = 0f, attackIntervalReduction = 0f, scaleMultiplier = 1.2f, overrideMaterial = golemTier1 },
                new UpgradeTier { label = "Golem II", cost = 8f, healthBonus = 40f, damageBonus = 5f, rangeBonus = 0f, attackIntervalReduction = 0.3f, scaleMultiplier = 1.35f, overrideMaterial = golemTier2 },
            };
        }

        if (sniperTiers == null || sniperTiers.Length == 0)
        {
            sniperTiers = new[]
            {
                new UpgradeTier { label = "Sniper I", cost = 4f, healthBonus = 10f, damageBonus = 3f, rangeBonus = 2f, attackIntervalReduction = 0.15f, scaleMultiplier = 1.2f, overrideMaterial = sniperTier1 },
                new UpgradeTier { label = "Sniper II", cost = 8f, healthBonus = 15f, damageBonus = 4f, rangeBonus = 2f, attackIntervalReduction = 0.5f, scaleMultiplier = 1.4f, overrideMaterial = sniperTier2 },
            };
        }

        if (raccoonTiers == null || raccoonTiers.Length == 0)
        {
            raccoonTiers = new[]
            {
                new UpgradeTier { label = "Raccoon I", cost = 5f, healthBonus = 50f, damageBonus = 1f, rangeBonus = 0f, attackIntervalReduction = 0f, scaleMultiplier = 1.4f, overrideMaterial = raccoonTier1, pulses = true },
                new UpgradeTier { label = "Raccoon II", cost = 9f, healthBonus = 70f, damageBonus = 2f, rangeBonus = 0f, attackIntervalReduction = 0f, scaleMultiplier = 1.75f, overrideMaterial = raccoonTier2, pulses = true },
            };
        }

        if (towerTiers == null || towerTiers.Length == 0)
        {
            towerTiers = new[]
            {
                new UpgradeTier { label = "Tower I", cost = 6f, healthBonus = 60f, damageBonus = 5f, rangeBonus = 2f, attackIntervalReduction = 0f, scaleMultiplier = 1.06f, overrideMaterial = towerTier1 },
                new UpgradeTier { label = "Tower II", cost = 10f, healthBonus = 90f, damageBonus = 8f, rangeBonus = 3f, attackIntervalReduction = 0.2f, scaleMultiplier = 1.1f, overrideMaterial = towerTier2 },
            };
        }
    }

    public int GetLevel(DefenderKind kind)
    {
        switch (kind)
        {
            case DefenderKind.Golem: return golemLevel;
            case DefenderKind.Sniper: return sniperLevel;
            case DefenderKind.Raccoon: return raccoonLevel;
            case DefenderKind.Tower: return towerLevel;
            default: return 0;
        }
    }

    public int MaxLevel(DefenderKind kind)
    {
        UpgradeTier[] tiers = GetTiers(kind);
        return tiers != null ? tiers.Length : 0;
    }

    public UpgradeTier GetTier(DefenderKind kind, int index)
    {
        UpgradeTier[] tiers = GetTiers(kind);

        if (tiers == null || index < 0 || index >= tiers.Length)
            return null;

        return tiers[index];
    }

    // Seconds left on this kind's current upgrade, or 0 if none is active —
    // what the upgrade button polls to draw its cooldown countdown.
    public float GetRemainingSeconds(DefenderKind kind)
    {
        float expireAt = GetExpireAt(kind);
        return expireAt > 0f ? Mathf.Max(0f, expireAt - Time.time) : 0f;
    }

    private UpgradeTier[] GetTiers(DefenderKind kind)
    {
        switch (kind)
        {
            case DefenderKind.Golem: return golemTiers;
            case DefenderKind.Sniper: return sniperTiers;
            case DefenderKind.Raccoon: return raccoonTiers;
            case DefenderKind.Tower: return towerTiers;
            default: return null;
        }
    }

    public bool TryPurchaseUpgrade(DefenderKind kind)
    {
        int currentLevel = GetLevel(kind);
        UpgradeTier[] tiers = GetTiers(kind);

        if (tiers == null || currentLevel >= tiers.Length)
        {
            Debug.Log($"{kind} is already at max upgrade level.");
            return false;
        }

        UpgradeTier nextTier = tiers[currentLevel];

        if (manaSystem == null || !manaSystem.TryUseMana(nextTier.cost))
        {
            Debug.Log($"Not enough mana to upgrade {kind}.");
            return false;
        }

        SetLevel(kind, currentLevel + 1);
        AudioManager.PlayButtonClick();
        Debug.Log($"{kind} upgraded to {nextTier.label} for {UpgradeDurationSeconds}s.");
        OnUpgradePurchased?.Invoke(kind, nextTier);

        RestartRevertTimer(kind);
        return true;
    }

    // (Re)starts this kind's 15-second countdown. Buying another tier of the
    // same kind before it finishes cancels the old countdown and starts a
    // fresh one — one shared window for however many tiers got stacked in.
    private void RestartRevertTimer(DefenderKind kind)
    {
        Coroutine existing = GetRevertRoutine(kind);
        if (existing != null) StopCoroutine(existing);

        SetExpireAt(kind, Time.time + UpgradeDurationSeconds);
        SetRevertRoutine(kind, StartCoroutine(RevertAfterDelay(kind)));
    }

    private IEnumerator RevertAfterDelay(DefenderKind kind)
    {
        yield return new WaitForSeconds(UpgradeDurationSeconds);

        SetLevel(kind, 0);
        SetExpireAt(kind, -1f);
        SetRevertRoutine(kind, null);

        Debug.Log($"{kind}'s upgrade timer ran out — reverting to base stats and colours.");
        OnUpgradeReverted?.Invoke(kind);
    }

    private void SetLevel(DefenderKind kind, int level)
    {
        switch (kind)
        {
            case DefenderKind.Golem: golemLevel = level; break;
            case DefenderKind.Sniper: sniperLevel = level; break;
            case DefenderKind.Raccoon: raccoonLevel = level; break;
            case DefenderKind.Tower: towerLevel = level; break;
        }
    }

    private float GetExpireAt(DefenderKind kind)
    {
        switch (kind)
        {
            case DefenderKind.Golem: return golemExpireAt;
            case DefenderKind.Sniper: return sniperExpireAt;
            case DefenderKind.Raccoon: return raccoonExpireAt;
            case DefenderKind.Tower: return towerExpireAt;
            default: return -1f;
        }
    }

    private void SetExpireAt(DefenderKind kind, float value)
    {
        switch (kind)
        {
            case DefenderKind.Golem: golemExpireAt = value; break;
            case DefenderKind.Sniper: sniperExpireAt = value; break;
            case DefenderKind.Raccoon: raccoonExpireAt = value; break;
            case DefenderKind.Tower: towerExpireAt = value; break;
        }
    }

    private Coroutine GetRevertRoutine(DefenderKind kind)
    {
        switch (kind)
        {
            case DefenderKind.Golem: return golemRevertRoutine;
            case DefenderKind.Sniper: return sniperRevertRoutine;
            case DefenderKind.Raccoon: return raccoonRevertRoutine;
            case DefenderKind.Tower: return towerRevertRoutine;
            default: return null;
        }
    }

    private void SetRevertRoutine(DefenderKind kind, Coroutine routine)
    {
        switch (kind)
        {
            case DefenderKind.Golem: golemRevertRoutine = routine; break;
            case DefenderKind.Sniper: sniperRevertRoutine = routine; break;
            case DefenderKind.Raccoon: raccoonRevertRoutine = routine; break;
            case DefenderKind.Tower: towerRevertRoutine = routine; break;
        }
    }
}

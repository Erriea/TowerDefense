using UnityEngine;
using UnityEngine.UI;
using TMPro;

// UPGRADE BUTTON — self-contained UI button for one defender type's (or the Tower's)
// upgrade track. Greys out past max level or when mana is short, same idiom as
// DefenderSpawner.DefenderButtonUI. While an upgrade is active it also shows a
// live countdown to when it reverts (UpgradeManager owns the actual timer —
// this just polls and displays it).
[RequireComponent(typeof(Button))]
public class UpgradeButton : MonoBehaviour
{
    [SerializeField] private DefenderKind kind;
    [SerializeField] private Mana manaSystem;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float unaffordableAlpha = 0.4f;

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
    }

    private void OnEnable()
    {
        if (manaSystem != null)
            manaSystem.OnManaChanged += HandleManaChanged;

        UpgradeManager.OnUpgradePurchased += HandleUpgradePurchased;
        UpgradeManager.OnUpgradeReverted += HandleUpgradeReverted;

        Refresh();
    }

    private void OnDisable()
    {
        if (manaSystem != null)
            manaSystem.OnManaChanged -= HandleManaChanged;

        UpgradeManager.OnUpgradePurchased -= HandleUpgradePurchased;
        UpgradeManager.OnUpgradeReverted -= HandleUpgradeReverted;
    }

    // The countdown number needs to tick every frame, not just on purchase/mana
    // events, so it stays live while the upgrade is active.
    private void Update()
    {
        Refresh();
    }

    private void HandleClick()
    {
        UpgradeManager.Instance?.TryPurchaseUpgrade(kind);
    }

    private void HandleManaChanged(float currentMana, float maxMana)
    {
        Refresh();
    }

    private void HandleUpgradePurchased(DefenderKind purchasedKind, UpgradeTier tier)
    {
        if (purchasedKind == kind)
            Refresh();
    }

    private void HandleUpgradeReverted(DefenderKind revertedKind)
    {
        if (revertedKind == kind)
            Refresh();
    }

    private void Refresh()
    {
        if (UpgradeManager.Instance == null) return;

        float remaining = UpgradeManager.Instance.GetRemainingSeconds(kind);

        // On cooldown — locked for the full 15s regardless of mana, independent
        // of every other kind's own cooldown.
        if (remaining > 0f)
        {
            if (statusText != null) statusText.text = $"{kind}\nActive ({remaining:0}s)";
            button.interactable = false;
            if (canvasGroup != null) canvasGroup.alpha = unaffordableAlpha;
            return;
        }

        int level = UpgradeManager.Instance.GetLevel(kind);
        int maxLevel = UpgradeManager.Instance.MaxLevel(kind);

        if (level >= maxLevel)
        {
            if (statusText != null) statusText.text = $"{kind}\nMAX";
            button.interactable = false;
            if (canvasGroup != null) canvasGroup.alpha = unaffordableAlpha;
            return;
        }

        UpgradeTier nextTier = UpgradeManager.Instance.GetTier(kind, level);
        float currentMana = manaSystem != null ? manaSystem.CurrentMana : 0f;
        bool canAfford = nextTier != null && currentMana >= nextTier.cost;

        if (statusText != null && nextTier != null)
            statusText.text = $"{kind} {level}/{maxLevel}\nCost = {nextTier.cost}";

        button.interactable = canAfford;
        if (canvasGroup != null) canvasGroup.alpha = canAfford ? 1f : unaffordableAlpha;
    }
}

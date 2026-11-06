using UnityEngine;
using UnityEngine.UI;
using TMPro;

// BOOST BUTTON — one button for one (DefenderKind, BoostType) combination.
// Shows cost normally; once bought, shows a live countdown and is fully
// locked (unclickable) until that exact boost ends — independent of every
// other button, including the same kind's other boost type.
[RequireComponent(typeof(Button))]
public class BoostButton : MonoBehaviour
{
    [SerializeField] private DefenderKind kind;
    [SerializeField] private BoostType boostType;
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

        BoostManager.OnBoostActivated += HandleBoostActivated;
        BoostManager.OnBoostEnded += HandleBoostEnded;

        Refresh();
    }

    private void OnDisable()
    {
        if (manaSystem != null)
            manaSystem.OnManaChanged -= HandleManaChanged;

        BoostManager.OnBoostActivated -= HandleBoostActivated;
        BoostManager.OnBoostEnded -= HandleBoostEnded;
    }

    // The countdown number needs to tick every frame, not just on
    // purchase/mana events, so it stays live while the boost is active.
    private void Update()
    {
        Refresh();
    }

    private void HandleClick()
    {
        BoostManager.Instance?.TryPurchaseBoost(kind, boostType);
    }

    private void HandleManaChanged(float currentMana, float maxMana)
    {
        Refresh();
    }

    private void HandleBoostActivated(DefenderKind activatedKind, BoostType activatedType, BoostDefinition definition)
    {
        if (activatedKind == kind && activatedType == boostType)
            Refresh();
    }

    private void HandleBoostEnded(DefenderKind endedKind, BoostType endedType)
    {
        if (endedKind == kind && endedType == boostType)
            Refresh();
    }

    private void Refresh()
    {
        if (BoostManager.Instance == null) return;

        BoostDefinition definition = BoostManager.Instance.GetDefinition(kind, boostType);
        string label = definition != null ? definition.label : $"{kind} {boostType}";
        float remaining = BoostManager.Instance.GetRemainingSeconds(kind, boostType);

        if (remaining > 0f)
        {
            if (statusText != null) statusText.text = $"{label}\nActive ({remaining:0}s)";
            button.interactable = false;
            if (canvasGroup != null) canvasGroup.alpha = unaffordableAlpha;
            return;
        }

        float currentMana = manaSystem != null ? manaSystem.CurrentMana : 0f;
        bool canAfford = definition != null && currentMana >= definition.cost;

        // Costs now vary per defender kind, so each button shows its own cost
        // again instead of relying on a single flat-cost heading.
        if (statusText != null)
            statusText.text = definition != null ? $"{label}\nCost = {definition.cost}" : label;

        button.interactable = canAfford;
        if (canvasGroup != null) canvasGroup.alpha = canAfford ? 1f : unaffordableAlpha;
    }
}

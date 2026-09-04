using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DefenderSpawner : MonoBehaviour
{
    [SerializeField] private Mana manaSystem;
    [SerializeField] private GameObject defenderPrefab;
    [SerializeField] private DefenderPlacementGenerator placementGenerator;

    [SerializeField] private float defenderCost = 2f;
    
    public float DefenderCost => defenderCost;

    private void OnEnable()
    {
        placementGenerator.OnSpotConfirmed += HandleSpotConfirmed;
    }

    private void OnDisable()
    {
        placementGenerator.OnSpotConfirmed -= HandleSpotConfirmed;
    }

    public void SelectDefender()
    {
        if (manaSystem.CurrentMana < defenderCost)
        {
            Debug.Log("Not enough mana to place defender!");
            return;
        }

        placementGenerator.BeginPlacementMode();
    }

    private void HandleSpotConfirmed(Vector3 position, Quaternion rotation)
    {
        if (!manaSystem.TryUseMana(defenderCost))
        {
            Debug.Log("Not enough mana!");
            return;
        }

        Instantiate(defenderPrefab, position, rotation);
    }
    
    public class DefenderButtonUI : MonoBehaviour
    {
        [SerializeField] private Mana manaSystem;
        [SerializeField] private DefenderSpawner defenderSpawner;
        [SerializeField] private Button defenderButton;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text costText;

        [SerializeField] private float unaffordableAlpha = 0.4f;

        private void OnEnable()
        {
            manaSystem.OnManaChanged += UpdateButtonState;

            costText.text = "Cost = " + defenderSpawner.DefenderCost;

            UpdateButtonState(manaSystem.CurrentMana, manaSystem.MaxMana);
        }

        private void OnDisable()
        {
            manaSystem.OnManaChanged -= UpdateButtonState;
        }

        private void UpdateButtonState(float currentMana, float maxMana)
        {
            bool canAfford = currentMana >= defenderSpawner.DefenderCost;

            defenderButton.interactable = canAfford;
            canvasGroup.alpha = canAfford ? 1f : unaffordableAlpha;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerHealthUI : MonoBehaviour
{
    [SerializeField] private Image healthFill;
    [SerializeField] private TMP_Text healthText;

    private void OnEnable()
    {
        Tower.OnTowerSpawned += HandleTowerSpawned;
    }

    private void OnDisable()
    {
        Tower.OnTowerSpawned -= HandleTowerSpawned;
    }

    private void HandleTowerSpawned(Tower tower)
    {
        tower.OnHealthChanged += UpdateHealthBar;
        UpdateHealthBar(tower.CurrentHealth, tower.MaxHealth);
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthFill.fillAmount = currentHealth / maxHealth;
        healthText.text = Mathf.CeilToInt(currentHealth) + " / " + Mathf.CeilToInt(maxHealth);
    }
}

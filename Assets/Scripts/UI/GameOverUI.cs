using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;

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
        tower.OnTowerDestroyed += ShowGameOver;
    }

    private void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
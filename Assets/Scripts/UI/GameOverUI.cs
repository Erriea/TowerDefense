using System.Collections;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private float gameOverDelay = 2f;

    private void OnEnable()
    {
        Tower.OnTowerSpawned += HandleTowerSpawned;
    }

    private void OnDisable()
    {
        Tower.OnTowerSpawned -= HandleTowerSpawned;
        Tower.OnTowerDestroyed -= ShowGameOver;
    }

    private void HandleTowerSpawned(Tower tower)
    {
        Tower.OnTowerDestroyed -= ShowGameOver;
        Tower.OnTowerDestroyed += ShowGameOver;
    }

    private void ShowGameOver()
    {
        StartCoroutine(ShowGameOverAfterDelay());
        // gameOverPanel.SetActive(true);
        // Time.timeScale = 0f;
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSecondsRealtime(gameOverDelay);

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }
}
using UnityEngine;
using TMPro;

public class GameStatsUI : MonoBehaviour
{
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private DefenderSpawner defenderSpawner;
    [SerializeField] private WaveManager waveManager;
    [SerializeField] private TMP_Text crowCountText;
    [SerializeField] private TMP_Text golemCountText;
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text enemiesRemainingText;

    private void OnEnable()
    {
        monsterSpawner.OnEnemySpawned += UpdateCrowCount;   // was: OnCrowSpawned
        defenderSpawner.OnDefenderPlaced += UpdateGolemCount;
        waveManager.OnWaveStarted += UpdateWaveNumber;
        waveManager.OnEnemiesRemainingChanged += UpdateEnemiesRemaining;

        UpdateCrowCount(0);
        UpdateGolemCount(0);
        UpdateWaveNumber(0);
        UpdateEnemiesRemaining(0);
    }

    private void OnDisable()
    {
        monsterSpawner.OnEnemySpawned -= UpdateCrowCount;   // was: OnCrowSpawned
        defenderSpawner.OnDefenderPlaced -= UpdateGolemCount;
        waveManager.OnWaveStarted -= UpdateWaveNumber;
        waveManager.OnEnemiesRemainingChanged -= UpdateEnemiesRemaining;
    }

    private void UpdateCrowCount(int count)
    {
        crowCountText.text = "Enemies Spawned: " + count;
    }

    private void UpdateGolemCount(int count)
    {
        golemCountText.text = "Golems Deployed: " + count;
    }

    private void UpdateWaveNumber(int wave)
    {
        waveText.text = "Wave: " + wave;
    }

    private void UpdateEnemiesRemaining(int count)
    {
        enemiesRemainingText.text = "Enemies Remaining: " + count;
    }
}
using UnityEngine;
using TMPro;

public class GameStatsUI : MonoBehaviour
{
    [SerializeField] private MonsterSpawner monsterSpawner;
    [SerializeField] private DefenderSpawner defenderSpawner;
    [SerializeField] private TMP_Text crowCountText;
    [SerializeField] private TMP_Text golemCountText;

    private void OnEnable()
    {
        monsterSpawner.OnCrowSpawned += UpdateCrowCount;
        defenderSpawner.OnDefenderPlaced += UpdateGolemCount;

        UpdateCrowCount(0);
        UpdateGolemCount(0);
    }

    private void OnDisable()
    {
        monsterSpawner.OnCrowSpawned -= UpdateCrowCount;
        defenderSpawner.OnDefenderPlaced -= UpdateGolemCount;
    }

    private void UpdateCrowCount(int count)
    {
        crowCountText.text = "Crows Spawned: " + count;
    }

    private void UpdateGolemCount(int count)
    {
        golemCountText.text = "Golems Deployed: " + count;
    }
}
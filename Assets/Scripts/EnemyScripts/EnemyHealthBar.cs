using UnityEngine;
using UnityEngine.UI;

// EnemyHealthBar.cs — sits on a world-space Canvas positioned above an enemy.
// Always faces the camera and reflects the enemy's current/max health.
// Wire the "healthFill" slot to the Image used as the bar's fill (Image Type: Filled).
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Image healthFill;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        // Billboard towards the camera so the bar reads face-on from any angle the enemy is walking.
        transform.forward = cam.transform.forward;
    }

    public void SetHealth(float current, float max)
    {
        if (healthFill == null || max <= 0f) return;
        healthFill.fillAmount = Mathf.Clamp01(current / max);
    }
}

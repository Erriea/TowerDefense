using System;
using UnityEngine;

public class Tower : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 100f;

    private float currentHealth;
    private bool isDead;

    public float MaxHealth => maxHealth;
    public float CurrentHealth => currentHealth;

    public event Action<float, float> OnHealthChanged;
    public static event Action OnTowerDestroyed;

    public static event Action<Tower> OnTowerSpawned;
    
    [SerializeField] private PostProcessingScreen screenEffect;

    private void Awake()
    {
        screenEffect = FindFirstObjectByType<PostProcessingScreen>();
        currentHealth = maxHealth;
        OnTowerSpawned?.Invoke(this);
    }

    private void Start()
    {
        Debug.Log("TOWER SCRIPT IS RUNNING!");
    }

    public void TakeDamage(float amount)
    {
        Debug.Log("TOWER TAKE DAMAGE CALLED!");

        if (isDead) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);
        Debug.Log($"Tower took {amount} damage, {currentHealth} / {maxHealth} HP left");
        AudioManager.PlayTowerCrack(transform.position);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        GetComponent<HitFeedback>()?.Flash();
        
        screenEffect?.PlayDamageEffect();

        if (currentHealth <= 0f)
        {
            Die();
            // isDead = true;
            //
            // //spawn destruction particles and debris
            // GetComponent<TowerDestruction>()?.DestroyTower();
            //
            // //Notify other systems
            // OnTowerDestroyed?.Invoke();
            //
            // //Destroys the original tower
            // var hitFeedback = GetComponent<HitFeedback>();
            //
            // if (hitFeedback != null)
            // {
            //     hitFeedback.Die(() => Destroy(gameObject));
            // }
            // else
            // {
            //     Destroy(gameObject);
            // }
        }
    }

    private void Die()
    {
        isDead = true;

        Debug.Log("TOWER HAS DIED!");

        // Spawn destruction effects BEFORE destroying tower
        TowerDestruction destruction =
            GetComponent<TowerDestruction>();

        if (destruction != null)
        {
            destruction.DestroyTower();
        }
        else
        {
            Debug.LogError("TowerDestruction component is missing!");
        }

        OnTowerDestroyed?.Invoke();

        // Destroy the original tower
        var hitFeedback = GetComponent<HitFeedback>();

        if (hitFeedback != null)
            hitFeedback.Die(() => Destroy(gameObject));
        else
            Destroy(gameObject);
    }
    
    // private void Die()
    // {
    //     isDead = true;
    //     
    //     OnTowerDestroyed?.Invoke();
    //
    //     var hitFeedback = GetComponent<HitFeedback>();
    //     if (hitFeedback != null)
    //         hitFeedback.Die(() => Destroy(gameObject));
    //     else
    //         Destroy(gameObject);
    // }
}
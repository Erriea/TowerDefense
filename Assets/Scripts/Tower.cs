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
    public event Action OnTowerDestroyed;

    public static event Action<Tower> OnTowerSpawned;

    private void Awake()
    {
        currentHealth = maxHealth;
        OnTowerSpawned?.Invoke(this);
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(currentHealth - amount, 0f);

        Debug.Log($"Tower took {amount} damage, {currentHealth} / {maxHealth} HP left");

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;
        OnTowerDestroyed?.Invoke();
        Destroy(gameObject);
    }
}
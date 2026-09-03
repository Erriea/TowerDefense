using UnityEngine;

// DEFENDER BASE CLASS
public abstract class Defender : MonoBehaviour
{
    [SerializeField] protected float maxHealth = 50f;
    [SerializeField] protected float attackDamage = 10f;
    [SerializeField] protected float attackRange = 5f;
    [SerializeField] protected float attackInterval = 1.5f;

    protected float currentHealth;
    protected float attackTimer;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Update()
    {
        attackTimer += Time.deltaTime;
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }
}

using UnityEngine;

// BulwarkDefender.cs — high health, low damage, slowly regenerates health over time
public class BulwarkDefender : Defender
{
    [SerializeField] private float healthRegenPerSecond = 1f;

    protected override void Update()
    {
        base.Update();

        if (currentHealth < maxHealth)
        {
            currentHealth = Mathf.Min(currentHealth + healthRegenPerSecond * Time.deltaTime, maxHealth);
        }
    }
}
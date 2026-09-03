using System;
using UnityEngine;
using UnityEngine.UI;


public class Mana : MonoBehaviour
{
    [Header("Mana")]
    [SerializeField] private float maxMana = 10f;
    [SerializeField] private float manaRegen = 2f;

    private float currentMana;
    
    public float CurrentMana => currentMana;
    public float MaxMana => maxMana;

    public event Action<float, float> OnManaChanged;
    
    private void Start()
    {
        currentMana = 0f;

        OnManaChanged?.Invoke(
            currentMana,
            maxMana
        );
    }
    
    private void Update()
    {
        GenerateMana();
    }
    
    private void GenerateMana()
    {
        if (currentMana >= maxMana)
            return;

        currentMana += manaRegen * Time.deltaTime;

        currentMana = Mathf.Clamp(
            currentMana,
            0f,
            maxMana
        );

        OnManaChanged?.Invoke(
            currentMana,
            maxMana
        );
    }
    
    public bool TryUseMana(float amount)
    {
        if (currentMana < amount)
            return false;

        currentMana -= amount;

        OnManaChanged?.Invoke(
            currentMana,
            maxMana
        );

        return true;
    }
    
    public void AddMana(float amount)
    {
        currentMana += amount;

        currentMana = Mathf.Clamp(
            currentMana,
            0f,
            maxMana
        );

        OnManaChanged?.Invoke(
            currentMana,
            maxMana
        );
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ManaUI : MonoBehaviour
{
    [SerializeField] private Mana manaSystem;
    [SerializeField] private Image manaFill;
    [SerializeField] private TMP_Text manaText;

    private void OnEnable()
    {
        manaSystem.OnManaChanged += UpdateManaBar;
    }

    private void OnDisable()
    {
        manaSystem.OnManaChanged -= UpdateManaBar;
    }

    private void UpdateManaBar(float currentMana, float maxMana)
    {
        float manaPercentage =
            currentMana / maxMana;

        manaFill.fillAmount = manaPercentage;
        
        manaText.text =
            Mathf.FloorToInt(currentMana).ToString();
    }
}

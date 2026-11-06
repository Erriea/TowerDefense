using TMPro;
using UnityEngine;

public class ProceduralDamageText : MonoBehaviour
{
    [SerializeField] private string[] damageWords = {"DANG!", "CRITICAL!", "FRACTURED!", "SHATTERED!", "SEEPING MANA!"};

    [SerializeField] private TextMeshPro damageText;

    public void GenerateDamageWord()
    {
        if (damageWords.Length == 0)
            return;

        int randomIndex = Random.Range(0, damageWords.Length);

        string generatedWord = damageWords[randomIndex];

        damageText.text = generatedWord;
    }
}

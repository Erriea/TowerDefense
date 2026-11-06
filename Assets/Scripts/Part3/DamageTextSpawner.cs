using TMPro;
using UnityEngine;

public class DamageTextSpawner : MonoBehaviour
{
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private string[] damageWords = {"DANG!", "CRITICAL!", "FRACTURED!", "SHATTERED!", "SEEPING MANA!"};
    
    public void SpawnDamageText()
    {
        if (damageTextPrefab == null || damageWords.Length == 0)
            return;

        //generate a random word
        int index = Random.Range(0, damageWords.Length);

        string generatedWord = damageWords[index];

        //spawns above the tower
        Vector3 spawnPosition = transform.position
                                + Vector3.up * 5f;

        GameObject textObject = Instantiate(
            damageTextPrefab,
            spawnPosition,
            Quaternion.identity
        );

        //assigns the generated word
        TextMeshPro text = textObject.GetComponent<TextMeshPro>();

        if (text != null)
        {
            text.text = generatedWord;
        }
    }
}

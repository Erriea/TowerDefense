using TMPro;
using UnityEngine;

public class FloatingDamageText : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 2f;
    [SerializeField] private float lifetime = 1.5f;

    private TextMeshPro text;
    private float timer;

    private void Awake()
    {
        text = GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        //move upwards
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        //fades out
        if (text != null)
        {
            Color color = text.color;

            color.a = Mathf.Lerp(
                1f,
                0f,
                timer / lifetime
            );

            text.color = color;
        }

        //destroy after lifetime
        if (timer >= lifetime)
        {
            Destroy(gameObject);
        }
    }
}

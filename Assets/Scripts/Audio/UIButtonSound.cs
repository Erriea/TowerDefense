using UnityEngine;
using UnityEngine.UI;

// Drop this on any UI Button to have it play the shared button-click SFX automatically —
// no per-button OnClick wiring needed, it hooks itself up at runtime.
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(AudioManager.PlayButtonClick);
    }
}

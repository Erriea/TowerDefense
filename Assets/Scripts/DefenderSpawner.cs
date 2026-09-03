using UnityEngine;

public class DefenderSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Mana manaSystem;
    [SerializeField] private GameObject defenderPrefab;
    [SerializeField] private Camera playerCamera;

    [Header("Defender Settings")]
    [SerializeField] private float defenderCost = 2f;

    private bool isPlacingDefender = false;

    private void Update()
    {
        if (!isPlacingDefender)
            return;

        HandlePlacement();
    }
    
    public void SelectDefender()
    {
        //ensures we have enough mana
        if (manaSystem.CurrentMana < defenderCost)
        {
            Debug.Log("Not enough mana to place defender!");
            return;
        }

        // Enter placement mode
        isPlacingDefender = true;

        Debug.Log("Defender selected. Click on the map to place it.");
    }

    private void HandlePlacement()
    {
        //checks if the player has pressed their left mouse click
        if (!Input.GetMouseButtonDown(0))
            return;

        //creates a ray from the camera through the mouse position 
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        //casts the ray onto the generated map and wiats for a 'hit'
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            PlaceDefender(hit.point);
        }
    }

    private void PlaceDefender(Vector3 position)
    {
        //checks if the player has enough mana to spawn the Golem
        if (!manaSystem.TryUseMana(defenderCost))
        {
            Debug.Log("Not enough mana!");
            isPlacingDefender = false;
            return;
        }

        //Spawns the defender
        Instantiate(
            defenderPrefab,
            position,
            Quaternion.identity
        );

        //once the player has placed the golem, we will exit the placing mode
        isPlacingDefender = false;

        Debug.Log("Defender placed!");
    }
}

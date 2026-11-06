using UnityEngine;

public class TowerDebris : MonoBehaviour
{
    [SerializeField] private float explosionForce = 8f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float upwardForce = 3f;
    
    void Start()
    {
        Rigidbody[] debris = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody debri in debris)
        {
            debri.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardForce, ForceMode.Impulse);
        }
        
        Destroy(gameObject, 5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

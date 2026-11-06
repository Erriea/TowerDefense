using UnityEngine;

public class TowerDestruction : MonoBehaviour
{
    [Header("Tower Destruction")] 
    [SerializeField] private GameObject destructionParticles;
    [SerializeField] private GameObject debrisPrefab;

    public void DestroyTower()
    {
        Debug.Log("Destroying Tower Destruction");
        //spawns the particles (for the dust vibes and all that)
        if (destructionParticles != null)
        {
            GameObject particles = Instantiate(destructionParticles, transform.position, Quaternion.identity);
            Debug.Log("PARTICLES SPAWNED: " + particles.name);

            ParticleSystem[] systems =
                particles.GetComponentsInChildren<ParticleSystem>(true);

            Debug.Log("PARTICLE SYSTEMS FOUND: " + systems.Length);

            foreach (ParticleSystem ps in systems)
            {
                ps.gameObject.SetActive(true);
                ps.Play(true);
                Debug.Log("PLAYING PARTICLE SYSTEM: " + ps.name);
            }
            
            Debug.Log("Tower position: " + transform.position);
            Debug.Log("Particle position: " + particles.transform.position);
        }
        else
        {
            Debug.LogError("No Destruction Particles Detected");
        }

        //spawns the bricks
        if (debrisPrefab != null)
        {
            GameObject debris = Instantiate(debrisPrefab, transform.position, Quaternion.identity);
            Debug.Log("DEBRIS SPAWNED: " + debris.name);
        }
        else
        {
            Debug.LogError("No Debris Prefab Detected");
        }
        
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

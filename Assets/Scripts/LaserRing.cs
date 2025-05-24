using UnityEngine;

public class LaserRing : MonoBehaviour
{
    public float duration = 5f;
    public float expandSpeed = 0.8f;
    public float damageRadius = 3.0f; 
    
    private float timer = 0f;
    private Vector3 initialScale;
   
    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Slowly expand
        float scaleFactor = 1f + expandSpeed * timer;
        transform.localScale = initialScale * scaleFactor;

        // Check player damage every frame
        CheckPlayerDamage();

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
    
    void CheckPlayerDamage()
    {
        // Get player head position (camera position)
        Transform playerHead = Camera.main?.transform;
        if (playerHead == null) return;
        
        // Get player health component
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth == null) return;
        
        // Get laser ring height
        float ringHeight = transform.position.y;
        
        // Get player head height
        float playerHeadHeight = playerHead.position.y;
        
        // Calculate horizontal distance between player head and laser ring
        Vector3 playerHeadXZ = new Vector3(playerHead.position.x, 0, playerHead.position.z);
        Vector3 ringXZ = new Vector3(transform.position.x, 0, transform.position.z);
        float horizontalDistance = Vector3.Distance(playerHeadXZ, ringXZ);
        
        // Check if player is in the damage range of the laser ring
        bool playerInRange = horizontalDistance < damageRadius * transform.localScale.x;
        
        // Simple height comparison: if player head is above the laser ring, deal damage
        bool playerIsStanding = playerHeadHeight > ringHeight;
        
        // If player is in range, standing, and not immune, deal damage
        if (playerInRange && playerIsStanding && !playerHealth.IsImmune())
        {
            playerHealth.TakeDamage();
        }
    }
}

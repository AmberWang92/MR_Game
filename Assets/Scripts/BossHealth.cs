using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manages the boss's health, health UI, and communicates with BossController when health thresholds are reached
/// </summary>
public class BossHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public Image healthBarImage;
    
    [Header("Events")]
    public float runAwayHealthThreshold = 0.5f; // Run away at 50% health
    
    // Internal variables
    private int currentHealth;
    private BossController bossController;
    private Animator animator;
    private bool isDead = false;
    
    void Start()
    {
        // Initialize health
        currentHealth = maxHealth;
        UpdateHealthBar();
        
        // Get required components
        bossController = GetComponent<BossController>();
        animator = GetComponentInChildren<Animator>();
        
        if (bossController == null)
        {
            Debug.LogError("BossController component not found on the same GameObject as BossHealth!");
        }
        
        if (animator == null)
        {
            Debug.LogWarning("No Animator component found in Boss or its children!");
        }
    }
    
    /// <summary>
    /// Updates the health bar UI to reflect current health
    /// </summary>
    private void UpdateHealthBar()
    {
        if (healthBarImage)
        {
            healthBarImage.fillAmount = (float)currentHealth / maxHealth;
        }
    }
    
    /// <summary>
    /// Public method to damage the boss
    /// </summary>
    /// <param name="amount">Amount of damage to apply</param>
    public void TakeDamage(int amount)
    {
        if (isDead) return;
        
        // Apply damage
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);
        UpdateHealthBar();
        
        // Log damage for debugging
        Debug.Log($"Boss took {amount} damage. Health: {currentHealth}/{maxHealth}");
        
        // Check health thresholds
        float healthPercentage = (float)currentHealth / maxHealth;
        
        if (currentHealth <= 0)
        {
            Die();
        }
        else if (healthPercentage <= runAwayHealthThreshold && bossController != null)
        {
            // Notify BossController to run away
            Debug.Log($"Boss health below {runAwayHealthThreshold * 100}%, triggering run away!");
            bossController.OnHealthThresholdReached(BossController.HealthThreshold.RunAway);
        }
        else if (bossController != null)
        {
            // Notify BossController to play damage animation
            bossController.OnHealthThresholdReached(BossController.HealthThreshold.TakeDamage);
        }
    }
    
    /// <summary>
    /// Handles boss death
    /// </summary>
    private void Die()
    {
        if (isDead) return;
        isDead = true;
        
        Debug.Log("Boss Defeated!");
        
        // Notify BossController about death
        if (bossController != null)
        {
            bossController.OnHealthThresholdReached(BossController.HealthThreshold.Death);
        }
        
        // Stop background music
        AudioSource bossMusic = GetComponent<AudioSource>();
        if (bossMusic != null && bossMusic.isPlaying)
        {
            StartCoroutine(FadeOutMusic(bossMusic, 2.0f));
        }
        
        // Schedule destruction
        Invoke("DestroyAfterAnimation", 3f);
    }
    
    /// <summary>
    /// Fades out the boss music
    /// </summary>
    private IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float startTime = Time.time;
        
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            audioSource.volume = Mathf.Lerp(startVolume, 0, t);
            yield return null;
        }
        
        audioSource.Stop();
        audioSource.volume = startVolume; 
    }
    
    /// <summary>
    /// Shows victory UI and destroys the boss GameObject
    /// </summary>
    private void DestroyAfterAnimation()
    {   
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictoryUI();
        }
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Returns the current health percentage (0-1)
    /// </summary>
    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
}

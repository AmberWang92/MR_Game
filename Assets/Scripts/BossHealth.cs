using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public Slider healthSlider;

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider) healthSlider.maxValue = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        if (healthSlider) healthSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Boss Defeated!");
        // TODO: Add death animation, victory UI, etc.
    }
}

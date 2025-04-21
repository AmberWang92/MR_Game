using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public Image healthBarImage;

    private int currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarImage) healthBarImage.fillAmount = maxHealth/100;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        if (healthBarImage) healthBarImage.fillAmount = currentHealth/100.0f;

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

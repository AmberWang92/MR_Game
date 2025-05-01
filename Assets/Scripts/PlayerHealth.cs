using Oculus.Haptics;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;

    public float restartDelay = 3f;

    public GameObject heartPrefab; // Reference to the heart prefab
    public Transform heartContainer; // Parent transform to hold heart instances
    private List<GameObject> hearts = new List<GameObject>();

    void Start()
    {
        currentLives = maxLives;
        InitializeHeartsUI();
        UpdateHeartsUI();
    }

    void InitializeHeartsUI()
    {
        for (int i = 0; i < maxLives; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartContainer);
            hearts.Add(heart);
        }
    }

    public void TakeDamage(int amount = 1)
    {
        currentLives -= amount;
        currentLives = Mathf.Max(currentLives, 0);

        Debug.Log("Player hit! Lives left: " + currentLives);
        GetComponent<HapticTrigger>().TriggerHaptics(OVRInput.Controller.RTouch);
        GetComponent<HapticTrigger>().TriggerHaptics(OVRInput.Controller.LTouch);

        UpdateHeartsUI();

        if (currentLives <= 0)
        {
            Die();
        }
    }

    void UpdateHeartsUI()
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].SetActive(i < currentLives);
        }
        Debug.Log("❤️ x " + currentLives);
    }

    public int GetLives()
    {
        return currentLives;
    }

    void Die()
    {
        Debug.Log("Player died!");
        UIManager.Instance.ShowDeathUI();
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

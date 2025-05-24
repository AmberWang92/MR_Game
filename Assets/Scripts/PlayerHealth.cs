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
    
    [Header("Damage Settings")]
    public float immunityDuration = 3.0f; 
    private float lastDamageTime = -10f;  
    private bool isImmune = false;       

    public GameObject heartPrefab; 
    public Transform heartContainer; 
    private List<GameObject> hearts = new List<GameObject>();

    [Header("UI Positioning")]
    public Transform bossTransform; 
    public Vector3 offsetFromBoss = new Vector3(-0.5f, 1.5f, 0f);
    public Transform playerCamera; 
    public float distanceFromBoss = 0.5f; 
    
    private bool isInitialized = false;

    void Start()
    {
        currentLives = maxLives;
        
        FindReferences();
        
        InitializeHeartsUI();

        UpdateHeartsUI();
        
        isInitialized = true;
    
        UpdateUIPosition();
    }

    void Update()
    {
        // If references are lost, try to re-find them
        if ((bossTransform == null || playerCamera == null) && isInitialized)
        {
            FindReferences();
        }

        UpdateUIPosition();
        
        UpdateImmunityState();
    }
    
    // Update immunity state
    private void UpdateImmunityState()
    {
        if (isImmune)
        {
            if (Time.time - lastDamageTime >= immunityDuration)
            {
                isImmune = false;
            }
        }
    }

    private void UpdateUIPosition()
    {
        if (bossTransform != null && playerCamera != null)
        {
            // Calculate the world position of the left-up corner of the boss
            Vector3 leftUp = bossTransform.position
                             + bossTransform.TransformDirection(offsetFromBoss.normalized) * distanceFromBoss
                             + offsetFromBoss;
            transform.position = leftUp;
            // Make the UI face the player camera
            transform.LookAt(transform.position + (transform.position - playerCamera.position));
        }
    }

    // In restart or initialization related methods
    public void Restart()
    {
        isInitialized = false;
        
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void FindReferences()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main?.transform;
        }
        
        if (bossTransform == null)
        {
            GameObject boss = GameObject.FindGameObjectWithTag("Boss");
            if (boss != null)
            {
                bossTransform = boss.transform;
            }
        }
        
        if (bossTransform == null)
        {
            Debug.LogWarning("PlayerHealth: Boss transform not found. UI positioning will not work correctly.");
        }
    }

    void InitializeHeartsUI()
    {
        // clear existing heart icons
        foreach (var heart in hearts)
        {
            if (heart != null) Destroy(heart);
        }
        hearts.Clear();

        // check necessary references
        if (heartPrefab == null)
        {
            return;
        }
        if (heartContainer == null)
        {
            return;
        }

        // instantiate new heart icons
        for (int i = 0; i < maxLives; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartContainer);
            heart.SetActive(true);
            hearts.Add(heart);
        }
    }

    public void TakeDamage()
    {
        if (isImmune)
        {
            return;
        }
        
        if (currentLives > 0)
        {
            isImmune = true;
            lastDamageTime = Time.time;
            
            currentLives--;
            UpdateHeartsUI();
            
            if (currentLives <= 0)
            {
                Die();
            }
            else
            {
                // 播放受伤音效或动画
                GetComponent<HapticTrigger>().TriggerHaptics(OVRInput.Controller.RTouch);
                GetComponent<HapticTrigger>().TriggerHaptics(OVRInput.Controller.LTouch);
            }
        }
    }

    void UpdateHeartsUI()
    {
        if (hearts.Count == 0)
        {
            return;
        }
        
        for (int i = 0; i < hearts.Count; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].SetActive(i < currentLives);
            }         
        }
        Debug.Log("❤️ x " + currentLives);
    }

    public int GetLives()
    {
        return currentLives;
    }
    
    public bool IsImmune()
    {
        return isImmune;
    }

    void Die()
    {
        Debug.Log("Player died!");
        
        UIManager.Instance.ShowDeathUI();
        
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
        {
            Animator bossAnimator = boss.GetComponentInChildren<Animator>();
            if (bossAnimator != null)
            {
                try
                {
                    bossAnimator.Play("surprised");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("播放Boss消失动画失败: " + e.Message);
                }
            }
            
            // delay destroying Boss object to allow animation to play
            Destroy(boss, 2f);
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

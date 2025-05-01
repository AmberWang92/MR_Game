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

    [Header("UI Positioning")]
    public Transform bossTransform; // Reference to the boss transform
    public Vector3 offsetFromBoss = new Vector3(-0.5f, 1.0f, 0f); // Offset from boss (left upper)
    public Transform playerCamera; // Reference to the player's camera
    public float distanceFromBoss = 0.5f; // Distance from boss
    
    // 添加一个标志，表示是否已经初始化
    private bool isInitialized = false;

    void Start()
    {
        // 初始化生命值
        currentLives = maxLives;
        
        // 初始化UI
        InitializeHeartsUI();
        UpdateHeartsUI();
        
        // 查找必要的引用
        FindReferences();
        
        // 标记为已初始化
        isInitialized = true;
    }
    
    // 查找必要的引用
    void FindReferences()
    {
        // 如果player camera未指定，尝试查找主相机
        if (playerCamera == null)
        {
            playerCamera = Camera.main?.transform;
        }
        
        // 如果boss transform未指定，尝试在场景中查找
        if (bossTransform == null)
        {
            // 尝试查找带有特定标签的boss
            GameObject bossObject = GameObject.FindGameObjectWithTag("Boss");
            if (bossObject != null)
            {
                bossTransform = bossObject.transform;
            }
            else
            {
                // 如果没有找到带有Boss标签的对象，可以尝试通过名称查找
                bossObject = GameObject.Find("Boss"); // 替换为你的boss对象名称
                if (bossObject != null)
                {
                    bossTransform = bossObject.transform;
                }
            }
        }
        
        // 如果仍然没有找到boss，输出警告
        if (bossTransform == null)
        {
            Debug.LogWarning("PlayerHealth: Boss transform not found. UI positioning will not work correctly.");
        }
    }

    void Update()
    {
        // 如果引用丢失，尝试重新查找
        if ((bossTransform == null || playerCamera == null) && isInitialized)
        {
            FindReferences();
        }
        
        // 更新UI位置和朝向
        if (bossTransform != null && playerCamera != null)
        {
            // Position the UI at the boss's left upper position
            Vector3 targetPosition = bossTransform.position + offsetFromBoss;
            
            // Update the position of the health UI
            transform.position = targetPosition;
            
            // Make the UI face the player camera
            transform.LookAt(2 * transform.position - playerCamera.position);
            
            // Alternative rotation method if the above doesn't work well
            // transform.rotation = Quaternion.LookRotation(transform.position - playerCamera.position);
        }
    }

    void InitializeHeartsUI()
    {
        // 清除现有的心形图标
        foreach (GameObject heart in hearts)
        {
            if (heart != null)
            {
                Destroy(heart);
            }
        }
        hearts.Clear();
        
        // 创建新的心形图标
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
        // 重置标志，以便在场景重新加载后重新初始化
        isInitialized = false;
        
        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

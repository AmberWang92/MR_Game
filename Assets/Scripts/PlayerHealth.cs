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
    public Vector3 offsetFromBoss = new Vector3(-0.5f, 1.5f, 0f); // Offset from boss (left upper)
    public Transform playerCamera; // Reference to the player's camera
    public float distanceFromBoss = 0.5f; // Distance from boss
    
    private bool isInitialized = false;

    void Start()
    {
        // 初始化生命值
        currentLives = maxLives;
        
        // 查找必要的引用
        FindReferences();
        
        // 初始化心形UI
        InitializeHeartsUI();
        
        // 更新心形UI显示
        UpdateHeartsUI();
        
        // 标记为已初始化
        isInitialized = true;
        
        // 更新UI位置
        UpdateUIPosition();
        
        Debug.Log("PlayerHealth初始化完成，生命值：" + currentLives);
    }

    void Update()
    {
        // 如果引用丢失，尝试重新查找
        if ((bossTransform == null || playerCamera == null) && isInitialized)
        {
            FindReferences();
        }
        // 每帧更新UI位置和朝向
        UpdateUIPosition();
    }

    // 统一的UI位置更新方法
    private void UpdateUIPosition()
    {
        if (bossTransform != null && playerCamera != null)
        {
            // 计算boss左上方的世界坐标
            Vector3 leftUp = bossTransform.position
                             + bossTransform.TransformDirection(offsetFromBoss.normalized) * distanceFromBoss
                             + offsetFromBoss;
            transform.position = leftUp;
            // 让UI面向玩家摄像机
            transform.LookAt(transform.position + (transform.position - playerCamera.position));
        }
    }

    // 在重启或初始化相关方法里调用
    public void Restart()
    {
        // 重置标志，以便在场景重新加载后重新初始化
        isInitialized = false;
        
        // 重新加载当前场景
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

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
            GameObject boss = GameObject.FindGameObjectWithTag("Boss");
            if (boss != null)
            {
                bossTransform = boss.transform;
            }
        }
        
        // 如果仍然没有找到boss，输出警告
        if (bossTransform == null)
        {
            Debug.LogWarning("PlayerHealth: Boss transform not found. UI positioning will not work correctly.");
        }
    }

    void InitializeHeartsUI()
    {
        // 清除现有的心形图标
        foreach (var heart in hearts)
        {
            if (heart != null) Destroy(heart);
        }
        hearts.Clear();

        // 检查必要的引用
        if (heartPrefab == null)
        {
            Debug.LogError("PlayerHealth: heartPrefab 未赋值！");
            return;
        }
        if (heartContainer == null)
        {
            Debug.LogError("PlayerHealth: heartContainer 未赋值！");
            return;
        }

        // 实例化新的心形图标
        for (int i = 0; i < maxLives; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartContainer);
            heart.SetActive(true);
            hearts.Add(heart);
            Debug.Log($"实例化Heart {i+1}/{maxLives}，父物体：{heart.transform.parent.name}");
        }

        Debug.Log($"初始化了 {hearts.Count} 个心形图标");
    }

    public void TakeDamage()
    {
        if (currentLives > 0)
        {
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
                Debug.Log("Player took damage! Lives remaining: " + currentLives);
            }
        }
    }

    void UpdateHeartsUI()
    {
        if (hearts.Count == 0)
        {
            Debug.LogWarning("UpdateHeartsUI: hearts列表为空！");
            return;
        }
        
        for (int i = 0; i < hearts.Count; i++)
        {
            if (hearts[i] != null)
            {
                hearts[i].SetActive(i < currentLives);
            }
            else
            {
                Debug.LogWarning($"UpdateHeartsUI: 第{i+1}个heart为null！");
            }
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

    public void QuitGame()
    {
        Application.Quit();
    }
}

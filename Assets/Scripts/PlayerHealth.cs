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
        // 如果引用丢失，尝试重新查找
        if ((bossTransform == null || playerCamera == null) && isInitialized)
        {
            FindReferences();
        }
        // 每帧更新UI位置和朝向
        UpdateUIPosition();
        
        // 更新免疫状态
        UpdateImmunityState();
    }
    
    // 更新免疫状态
    private void UpdateImmunityState()
    {
        // 如果当前处于免疫状态，检查是否已经过了免疫时间
        if (isImmune)
        {
            if (Time.time - lastDamageTime >= immunityDuration)
            {
                isImmune = false;
            }
        }
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
            return;
        }
        if (heartContainer == null)
        {
            return;
        }

        // 实例化新的心形图标
        for (int i = 0; i < maxLives; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartContainer);
            heart.SetActive(true);
            hearts.Add(heart);
        }
    }

    public void TakeDamage()
    {
        // 如果处于免疫状态，不受伤害
        if (isImmune)
        {
            return;
        }
        
        if (currentLives > 0)
        {
            // 设置免疫状态
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
        
        // 先显示死亡UI
        UIManager.Instance.ShowDeathUI();
        
        // 找到场景中的Boss并让它消失
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
        {
            // 尝试播放消失动画
            Animator bossAnimator = boss.GetComponentInChildren<Animator>();
            if (bossAnimator != null)
            {
                try
                {
                    bossAnimator.Play("surprised");
                    Debug.Log("播放Boss消失动画");
                }
                catch (System.Exception e)
                {
                    Debug.LogError("播放Boss消失动画失败: " + e.Message);
                }
            }
            
            // 延迟销毁Boss对象，给动画留出播放时间
            Destroy(boss, 2f);
            Debug.Log("Boss将在2秒后消失");
        }
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}

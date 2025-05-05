using UnityEngine;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public GameObject deathUI;
    public GameObject victoryUI;
    public Transform bossTransform;
    public Transform playerTransform;
    
    
    [Header("UI Position Settings")]
    public float distanceFromPlayer = 1.0f; 
    public float heightOffset = 0.8f;  
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 初始化playerTransform
        if (playerTransform == null)
        {
            playerTransform = Camera.main.transform;
        }
        
        HideDeathUI();
        HideVictoryUI();
    }


    void Update()
    {
        // 只有 deathUI 激活时才同步
        if (deathUI != null && deathUI.activeSelf && playerTransform != null)
        {
            UpdateUIPosition(deathUI);
        }

        if (victoryUI != null && victoryUI.activeSelf && playerTransform != null)
        {
            UpdateUIPosition(victoryUI);
        }
    }
    
    // 提取UI位置更新为单独的方法，避免代码重复
    private void UpdateUIPosition(GameObject ui)
    {
        // 获取玩家的前方向量（相机朝向）
        Vector3 playerForward = playerTransform.forward;
        playerForward.y = 0; // 保持水平方向
        playerForward.Normalize();
        
        // 计算UI的位置：玩家前方一定距离，并有一定高度
        Vector3 uiPosition = playerTransform.position + playerForward * distanceFromPlayer;
        uiPosition.y = playerTransform.position.y + heightOffset; // 将UI放在玩家视线高度
        
        // 设置UI位置
        ui.transform.position = uiPosition;
        
        // 计算朝向玩家的旋转 - 使用LookAt而不是LookRotation
        ui.transform.LookAt(playerTransform.position);
        
        // 旋转180度，让UI的正面朝向玩家
        ui.transform.Rotate(0, 180, 0);
        
        // 应用额外的旋转调整 - 只调整X轴
        ui.transform.Rotate(15, 0, 0);
    }

    public void ShowDeathUI()
    {
        // 获取 Boss Transform
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
        {
            bossTransform = boss.transform;
        }

        if (deathUI != null)
            deathUI.SetActive(true);
    }

    public void HideDeathUI()
    {
        if (deathUI != null)
            deathUI.SetActive(false);
    }

    public void HideVictoryUI()
    {
        if (victoryUI != null)
            victoryUI.SetActive(false);
    }

    public void ShowVictoryUI()
    {
        // 获取 Boss Transform，与ShowDeathUI保持一致
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
        {
            bossTransform = boss.transform;
        }
        
        if (victoryUI != null)
            victoryUI.SetActive(true);
    }
}

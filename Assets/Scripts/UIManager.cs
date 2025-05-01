using UnityEngine;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public GameObject deathUI;
    public Transform bossTransform;
    public Transform playerTransform;
    
    // 可调整的UI位置参数
    [Header("UI位置设置")]
    public float distanceFromPlayer = 1.0f; // UI与玩家的距离，从1.5米减少到1.0米
    public float heightOffset = 0.8f;      // UI的高度偏移，从1.5米减少到0.8米
    

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
        HideDeathUI();
    }


    void Update()
    {
        // 只有 deathUI 激活时才同步
        if (deathUI != null && deathUI.activeSelf && playerTransform != null)
        {
            // 获取玩家的前方向量（相机朝向）
            Vector3 playerForward = playerTransform.forward;
            playerForward.y = 0; // 保持水平方向
            playerForward.Normalize();
            
            // 计算UI的位置：玩家前方一定距离，并有一定高度
            Vector3 uiPosition = playerTransform.position + playerForward * distanceFromPlayer;
            uiPosition.y = playerTransform.position.y + heightOffset; // 将UI放在玩家视线高度
            
            // 设置UI位置
            deathUI.transform.position = uiPosition;
            
            // 使UI面向玩家（看向玩家）
            deathUI.transform.LookAt(new Vector3(playerTransform.position.x, 
                                                uiPosition.y, // 保持同一高度看向玩家
                                                playerTransform.position.z));
            
            // 旋转180度，让UI的正面朝向玩家
            deathUI.transform.Rotate(0, 180, 0);
            
            // 可选：微调UI角度，使其更容易阅读
            deathUI.transform.Rotate(15, 0, 0); // 稍微向下倾斜15度，便于阅读
        }
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
}

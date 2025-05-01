using UnityEngine;

public class LaserRing : MonoBehaviour
{
    public float duration = 5f;         // 激光持续时间
    public float expandSpeed = 0.8f;    // 每秒扩大多少倍
    public float crouchHeightThreshold = 1.0f; // 判定为蹲下的高度阈值
    public float damageInterval = 2.0f; // 伤害间隔时间，避免连续造成伤害
    
    private float timer = 0f;
    private Vector3 initialScale;
    private float lastDamageTime = -1f; // 上次造成伤害的时间
   
    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 缓慢扩大
        float scaleFactor = 1f + expandSpeed * timer;
        transform.localScale = initialScale * scaleFactor;

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        CheckAndDamagePlayer(other);
    }
    
    // 添加持续检测，这样当玩家蹲下时可以实时更新状态
    void OnTriggerStay(Collider other)
    {
        CheckAndDamagePlayer(other);
    }
    
    void CheckAndDamagePlayer(Collider other)
    {
        // 检查冷却时间
        if (Time.time - lastDamageTime < damageInterval)
        {
            return; // 还在冷却中，不造成伤害
        }
        
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }
        
        if (playerHealth != null)
        {
            // 检查玩家是否蹲下
            bool playerIsCrouching = IsPlayerCrouching(other);
            
            // 如果玩家没有蹲下，才造成伤害
            if (!playerIsCrouching)
            {
                playerHealth.TakeDamage();
                lastDamageTime = Time.time; // 更新上次造成伤害的时间
            }
        }
    }
    
    // 检查玩家是否蹲下
    bool IsPlayerCrouching(Collider playerCollider)
    {
        // 方法1：检查碰撞体高度
        CapsuleCollider capsule = playerCollider as CapsuleCollider;
        if (capsule != null)
        {
            // 如果碰撞体高度小于阈值，认为玩家蹲下了
            return capsule.height < crouchHeightThreshold;
        }
        
        // 方法2：尝试获取PlayerColliderController组件
        PlayerColliderController colliderController = playerCollider.GetComponent<PlayerColliderController>();
        if (colliderController != null && colliderController.capsuleCollider != null)
        {
            return colliderController.capsuleCollider.height < crouchHeightThreshold;
        }
        
        // 方法3：检查碰撞体的世界空间高度
        // 计算碰撞体顶部与激光的相对高度
        float colliderTopY = playerCollider.bounds.max.y;
        float laserY = transform.position.y;
        
        // 如果碰撞体顶部低于激光，认为玩家蹲下了躲避激光
        return colliderTopY < laserY;
    }
}

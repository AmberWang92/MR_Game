using UnityEngine;

public class LaserRing : MonoBehaviour
{
    public float duration = 5f;
    public float expandSpeed = 0.8f;
    public float crouchHeightThreshold = 0.5f;
    
    private float timer = 0f;
    private Vector3 initialScale;
   
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
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<PlayerHealth>();
        }
        
        if (playerHealth != null)
        {
            // 检查玩家是否蹲下
            bool playerIsCrouching = IsPlayerCrouching(other);
            
            // 如果玩家没有蹲下且不处于免疫状态，才造成伤害
            if (!playerIsCrouching && !playerHealth.IsImmune())
            {
                playerHealth.TakeDamage();
            }
        }
    }
    
    // 检查玩家是否蹲下
    bool IsPlayerCrouching(Collider playerCollider)
    {
        // 方法1：优先使用PlayerColliderController.IsPlayerSquatting方法
        PlayerColliderController colliderController = playerCollider.GetComponent<PlayerColliderController>();
        if (colliderController == null)
        {
            colliderController = playerCollider.GetComponentInParent<PlayerColliderController>();
        }
        
        if (colliderController != null)
        {
            return colliderController.IsPlayerSquatting();
        }
        
        // 方法2：检查碰撞体高度（后备方法）
        CapsuleCollider capsule = playerCollider as CapsuleCollider;
        if (capsule != null)
        {
            // 如果碰撞体高度小于阈值，认为玩家蹲下了
            return capsule.height < crouchHeightThreshold;
        }
        
        // 方法3：检查碰撞体的世界空间高度（最后的后备方法）
        // 计算碰撞体顶部与激光的相对高度
        float colliderTopY = playerCollider.bounds.max.y;
        float laserY = transform.position.y;
        
        // 如果碰撞体顶部低于激光，认为玩家蹲下了躲避激光
        return colliderTopY < laserY;
    }
}

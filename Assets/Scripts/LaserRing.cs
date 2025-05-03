using UnityEngine;

public class LaserRing : MonoBehaviour
{
    public float duration = 5f;
    public float expandSpeed = 0.8f;
    public float damageRadius = 3.0f;  // 伤害检测半径
    
    private float timer = 0f;
    private Vector3 initialScale;
    private float lastCheckTime = 0f;  // 上次检查时间
   
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

        // 每帧检测玩家
        CheckPlayerDamage();

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
    
    void CheckPlayerDamage()
    {
        // 获取玩家头部位置（相机位置）
        Transform playerHead = Camera.main?.transform;
        if (playerHead == null) return;
        
        // 获取玩家健康组件
        PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth == null) return;
        
        // 获取激光环的高度
        float ringHeight = transform.position.y;
        
        // 获取玩家头部高度
        float playerHeadHeight = playerHead.position.y;
        
        // 计算玩家头部与激光环的水平距离
        Vector3 playerHeadXZ = new Vector3(playerHead.position.x, 0, playerHead.position.z);
        Vector3 ringXZ = new Vector3(transform.position.x, 0, transform.position.z);
        float horizontalDistance = Vector3.Distance(playerHeadXZ, ringXZ);
        
        // 调试输出
        //Debug.Log($"Ring Height: {ringHeight}, Player Height: {playerHeadHeight}, Distance: {horizontalDistance}");
        
        // 检查玩家是否在激光环的伤害范围内
        bool playerInRange = horizontalDistance < damageRadius * transform.localScale.x;
        
        // 简单的高度比较：如果玩家头部高于激光环，则造成伤害
        bool playerIsStanding = playerHeadHeight > ringHeight;
        
        // 如果玩家在范围内，站立且不处于免疫状态，则造成伤害
        if (playerInRange && playerIsStanding && !playerHealth.IsImmune())
        {
            playerHealth.TakeDamage();
        }
    }
    
    //// 保留这些方法用于可视化调试
    //void OnTriggerEnter(Collider other)
    //{
    //    Debug.Log($"触发器进入: {other.name}");
    //}
    
    //void OnTriggerStay(Collider other)
    //{
    //    Debug.Log($"触发器停留: {other.name}");
    //}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayGunSpawner : MonoBehaviour
{
    [Header("射线枪设置")]
    public GameObject rayGunPrefab; // 射线枪预制体
    
    [Header("生成区域设置")]
    public float spawnHeight = 1.0f; // 生成高度
    public float spawnRadius = 8.0f; // 生成半径（以玩家为中心）
    public float minDistanceFromBoss = 5.0f; // 与Boss的最小距离
    
    private Transform bossTransform; // Boss的Transform
    
    void Start()
    {
        // 查找Boss
        GameObject boss = GameObject.FindWithTag("Boss");
        if (boss != null)
        {
            bossTransform = boss.transform;
            Debug.Log("找到Boss，位置: " + bossTransform.position);
        }
        else
        {
            Debug.LogWarning("未找到Boss！请确保Boss有'Boss'标签。");
        }
        
        // 检查射线枪预制体
        if (rayGunPrefab == null)
        {
            Debug.LogError("射线枪预制体未分配！请在Inspector中分配。");
            return;
        }
        
        // 生成射线枪
        SpawnRayGun();
    }
    
    // 生成射线枪
    public void SpawnRayGun()
    {
        if (rayGunPrefab == null) return;
        
        // 尝试最多10次找到合适的位置
        for (int i = 0; i < 10; i++)
        {
            // 获取随机位置
            Vector3 spawnPosition = GetRandomSpawnPosition();
            
            // 检查与Boss的距离
            if (bossTransform != null)
            {
                float distanceToBoss = Vector3.Distance(
                    new Vector3(spawnPosition.x, 0, spawnPosition.z),
                    new Vector3(bossTransform.position.x, 0, bossTransform.position.z)
                );
                
                // 如果太靠近Boss，尝试下一个位置
                if (distanceToBoss < minDistanceFromBoss)
                {
                    Debug.Log($"位置 {spawnPosition} 太靠近Boss，重新尝试...");
                    continue;
                }
            }
            
            // 生成射线枪
            GameObject rayGun = Instantiate(rayGunPrefab, spawnPosition, Quaternion.identity);
            Debug.Log($"在 {spawnPosition} 生成了射线枪");
            return;
        }
        
        // 如果所有尝试都失败，在玩家前方生成
        Vector3 fallbackPosition = Camera.main.transform.position + Camera.main.transform.forward * 2.0f;
        fallbackPosition.y = spawnHeight;
        
        GameObject fallbackGun = Instantiate(rayGunPrefab, fallbackPosition, Quaternion.identity);
        Debug.Log($"在备用位置 {fallbackPosition} 生成了射线枪");
    }
    
    // 获取随机生成位置
    private Vector3 GetRandomSpawnPosition()
    {
        // 获取玩家位置
        Vector3 playerPosition = Camera.main.transform.position;
        
        // 在玩家周围的圆形区域内随机选择一个点
        float angle = Random.Range(0f, Mathf.PI * 2);
        float distance = Random.Range(2.0f, spawnRadius);
        
        float x = playerPosition.x + Mathf.Cos(angle) * distance;
        float z = playerPosition.z + Mathf.Sin(angle) * distance;
        
        // 返回生成位置
        return new Vector3(x, spawnHeight, z);
    }
    
    // 在Scene视图中可视化生成区域
    void OnDrawGizmosSelected()
    {
        if (Camera.main != null)
        {
            Vector3 playerPosition = Camera.main.transform.position;
            
            // 绘制生成半径
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(playerPosition.x, spawnHeight, playerPosition.z), spawnRadius);
        }
        
        // 绘制Boss安全区域
        if (bossTransform != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bossTransform.position, minDistanceFromBoss);
        }
    }
}
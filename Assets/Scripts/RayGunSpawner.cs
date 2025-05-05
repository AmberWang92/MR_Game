using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayGunSpawner : MonoBehaviour
{
    [Header("Ray Gun Prefab")]
    public GameObject rayGunPrefab; 
    
    [Header("Spawn Area Settings")]
    public float spawnHeight = 2.0f; 
    public float spawnRadius = 1.0f; 
    public float minDistanceFromBoss = 0.5f;
    
    [Header("Spawn Time Settings")]
    public float initialSpawnDelay = 20.0f; 
    public float respawnTime = 60.0f; 
    
    private Transform bossTransform;
    
    private void Start()
    {
        // 查找Boss
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
        {
            bossTransform = boss.transform;
        }
        else
        {
            Debug.LogWarning("找不到Boss，请确保Boss有'Boss'标签");
        }
        
        // 延迟生成第一把射线枪
        StartCoroutine(DelayedSpawn());
    }
    
    // 延迟生成第一把射线枪
    private IEnumerator DelayedSpawn()
    {
        Debug.Log($"射线枪将在 {initialSpawnDelay} 秒后生成");
        yield return new WaitForSeconds(initialSpawnDelay);
        SpawnRayGun();
    }
    
    // 生成射线枪
    public void SpawnRayGun()
    {
        if (rayGunPrefab == null)
        {
            Debug.LogError("未设置射线枪预制体！");
            return;
        }
        
        // 尝试找到合适的生成位置
        Vector3 spawnPosition = Vector3.zero;
        bool foundPosition = false;
        
        // 尝试多次找到合适的位置
        for (int attempt = 0; attempt < 10; attempt++)
        {
            Vector3 potentialPosition = GetRandomSpawnPosition();
            
            // 检查与Boss的距离
            if (bossTransform != null)
            {
                float distanceToBoss = Vector3.Distance(potentialPosition, bossTransform.position);
                if (distanceToBoss < minDistanceFromBoss)
                {
                    // 太靠近Boss，尝试下一个位置
                    continue;
                }
            }
            
            // 检查是否有其他物体阻挡
            if (!Physics.CheckSphere(potentialPosition, 0.3f))
            {
                spawnPosition = potentialPosition;
                foundPosition = true;
                break;
            }
        }
        
        // 如果找不到合适的位置，使用备用方案
        if (!foundPosition)
        {
            Debug.LogWarning("无法找到合适的射线枪生成位置，使用备用位置");
            spawnPosition = GetFallbackPosition();
        }
        
        // 生成射线枪
        GameObject rayGun = Instantiate(rayGunPrefab, spawnPosition, Quaternion.identity);
        Debug.Log($"射线枪已生成在位置: {spawnPosition}");
        
        // 监听射线枪的销毁事件，以便在适当的时候重新生成
        StartCoroutine(MonitorRayGun(rayGun));
    }
    
    // 监控射线枪，当它被销毁时重新生成
    private IEnumerator MonitorRayGun(GameObject rayGun)
    {
        // 等待射线枪被销毁或拾取
        while (rayGun != null)
        {
            yield return new WaitForSeconds(5.0f); // 每5秒检查一次
        }
        
        // 射线枪已被销毁，等待一段时间后重新生成
        Debug.Log($"射线枪已被拾取或销毁，将在 {respawnTime} 秒后重新生成");
        yield return new WaitForSeconds(respawnTime);
        SpawnRayGun();
    }
    
    // 获取随机生成位置
    private Vector3 GetRandomSpawnPosition()
    {
        // 在圆形区域内随机一个位置
        float angle = Random.Range(0f, Mathf.PI * 2);
        float distance = Random.Range(0f, spawnRadius);
        
        // 计算XZ平面上的位置
        float x = Mathf.Cos(angle) * distance;
        float z = Mathf.Sin(angle) * distance;
        
        // 获取玩家位置作为参考
        Vector3 playerPosition = Vector3.zero;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerPosition = player.transform.position;
        }
        
        // 最终位置 = 玩家位置 + 随机偏移 + 高度
        return new Vector3(
            playerPosition.x + x,
            playerPosition.y + spawnHeight,
            playerPosition.z + z
        );
    }
    
    // 获取备用生成位置（在玩家前方）
    private Vector3 GetFallbackPosition()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // 在玩家前方生成
            Vector3 playerForward = player.transform.forward;
            Vector3 spawnPosition = player.transform.position + playerForward * 2.0f;
            spawnPosition.y = player.transform.position.y + spawnHeight;
            return spawnPosition;
        }
        else
        {
            // 如果找不到玩家，使用默认位置
            return new Vector3(0, spawnHeight, 0);
        }
    }
}
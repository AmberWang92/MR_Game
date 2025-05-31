using System.Collections;
using UnityEngine;

/// <summary>
/// 负责Boss的攻击行为和效果
/// </summary>
public class BossAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public float attackInterval = 8f;
    public GameObject laserRingPrefab;
    public Transform laserSpawnPoint;
    public float laserDuration = 5f;

    private Animator animator;
    private float attackTimer = 0f;

    void Start()
    {
        // 获取动画控制器
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("BossAttack: No Animator component found in Boss or its children!");
        }
        
        // 确保有激光生成点
        if (laserSpawnPoint == null)
        {
            laserSpawnPoint = transform;
            Debug.LogWarning("BossAttack: No laser spawn point assigned, using boss transform instead.");
        }
    }

    void Update()
    {
        // 更新攻击计时器
        attackTimer += Time.deltaTime;
    }

    /// <summary>
    /// 检查是否可以攻击（攻击间隔已到）
    /// </summary>
    public bool CanAttack()
    {
        return attackTimer >= attackInterval;
    }

    /// <summary>
    /// 重置攻击计时器
    /// </summary>
    public void ResetAttackTimer()
    {
        attackTimer = 0f;
    }

    /// <summary>
    /// 执行攻击动作
    /// </summary>
    public void PerformAttack()
    {
        StartCoroutine(FireLaserRing());
        ResetAttackTimer();
    }

    /// <summary>
    /// 生成激光环攻击
    /// </summary>
    private IEnumerator FireLaserRing()
    {
        // 触发攻击动画
        if (animator != null)
        {
            animator.Play("attack");
        }

        // 生成激光环
        GameObject laser = Instantiate(laserRingPrefab, laserSpawnPoint.position, Quaternion.identity);
        laser.transform.rotation = Quaternion.LookRotation(Vector3.up);
        Debug.Log("Laser Ring spawned at: " + laserSpawnPoint.position);

        // 如果需要，可以在这里添加激光环的销毁逻辑
        // Destroy(laser, laserDuration);

        yield return null;
    }
}

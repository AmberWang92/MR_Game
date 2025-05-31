using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using UnityEngine.AI;
using Unity.AI.Navigation;

/// <summary>
/// Controls the boss's behavior, movement, and state management
/// </summary>
public class BossController : MonoBehaviour
{
    [Header("Navigation")]
    public NavMeshAgent navMeshAgent;
    
    // 引用其他组件
    private Component bossAttackComponent; // 将在运行时获取BossAttack组件
    
    // Health threshold enum for communication with BossHealth
    public enum HealthThreshold { TakeDamage, RunAway, Death }
    
    // FSM States
    public enum BossState { Idle, Attack, RunAway, TakeDamage, Defeat }
    public BossState currentState = BossState.Idle;
    
    // Internal variables
    private float stateTimer = 0f;
    private bool isDefeated = false;
    private Transform cameraTransform;
    private Animator animator;
    private AudioSource backgroundMusic;
    private BossHealth bossHealth;

    void Start()
    {
        // Initialize state
        currentState = BossState.Idle;
        stateTimer = 0f;

        // Get references
        cameraTransform = Camera.main.transform;
        animator = GetComponentInChildren<Animator>();
        bossHealth = GetComponent<BossHealth>();
        bossAttackComponent = GetComponent("BossAttack"); // 通过字符串获取组件
        
        if (animator == null)
        {
            Debug.LogWarning("No Animator component found in Boss or its children!");
        }
        
        if (bossHealth == null)
        {
            Debug.LogError("BossHealth component not found! Add it to the same GameObject.");
        }
        
        if (bossAttackComponent == null)
        {
            Debug.LogError("BossAttack component not found! Add it to the same GameObject.");
        }

        // Setup background music
        SetupBackgroundMusic();

        // Register scene loaded callback
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
    }
    
    /// <summary>
    /// Sets up the background music for the boss
    /// </summary>
    private void SetupBackgroundMusic()
    {
        backgroundMusic = gameObject.AddComponent<AudioSource>();
        
        AudioClip bossMusic = Resources.Load<AudioClip>("BossMusic");
        if (bossMusic != null)
        {
            backgroundMusic.clip = bossMusic;
            backgroundMusic.loop = true;
            backgroundMusic.volume = 0.1f;
            backgroundMusic.Play();
            Debug.Log("Boss Music started");
        }
        else
        {
            Debug.LogWarning("Boss Music not found");
        }
    }

    void OnSceneLoaded()
    {
        StartCoroutine(DelayedNavMeshAndSpawn());
    }

    // 延迟协程，等待障碍物生成后再烘焙NavMesh和采样出生点
    private IEnumerator DelayedNavMeshAndSpawn()
    {
        // 等待2帧结束，确保障碍物已生成
        //yield return new WaitForEndOfFrame();
        //yield return new WaitForEndOfFrame();

        // Get current room and floor anchor
        var currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom == null || currentRoom.FloorAnchor == null)
        {
            Debug.LogWarning("No room or floor anchor found!");
            yield break;
        }

        NavMeshSurface[] surfaces = FindObjectsByType<NavMeshSurface>(FindObjectsSortMode.None);
        surfaces[0].BuildNavMesh(); // 运行时烘焙

        // Get player forward direction
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        // Calculate target position: 1.5m away from player, 0.3m above floor
        Vector3 targetPos = cameraTransform.position + forward * 1.5f;
        var floorAnchor = currentRoom.FloorAnchor;
        targetPos.y = floorAnchor.transform.position.y + 0.3f;

        // NavMesh采样，确保出生点在可行走区域且避开障碍物
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPos, out hit, 1.0f, NavMesh.AllAreas))
        {
            targetPos = hit.position;
        }
        else
        {
            Debug.LogWarning("No valid NavMesh position found for Boss spawn, using fallback.");
        }

        // Move Boss to that position
        transform.position = targetPos;

        Debug.Log("Boss moved to position: " + targetPos);
    }

    void Update()
    {
        if (isDefeated) return;
        
        // Update timers
        stateTimer += Time.deltaTime;

        // Always face the player
        Vector3 lookAt = new Vector3(cameraTransform.position.x, transform.position.y, cameraTransform.position.z);
        transform.LookAt(lookAt);

        // Handle current state
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState();
                break;
            case BossState.Attack:
                HandleAttackState();
                break;
            case BossState.RunAway:
                HandleRunAwayState();
                break;
            case BossState.TakeDamage:
                HandleTakeDamageState();
                break;
            case BossState.Defeat:
                HandleDefeatState();
                break;
        }

        // Debug: Respawn boss when space is pressed
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            Debug.Log("---------- Space Button Pressed");
            StartCoroutine(DelayedNavMeshAndSpawn());
        }
    }
    
    /// <summary>
    /// Called by BossHealth when health thresholds are reached
    /// </summary>
    public void OnHealthThresholdReached(HealthThreshold threshold)
    {
        switch (threshold)
        {
            case HealthThreshold.TakeDamage:
                if (currentState != BossState.Defeat && currentState != BossState.RunAway)
                {
                    ChangeState(BossState.TakeDamage);
                }
                break;
                
            case HealthThreshold.RunAway:
                if (currentState != BossState.Defeat && currentState != BossState.RunAway)
                {
                    ChangeState(BossState.RunAway);
                    Debug.Log("Boss is running away!");
                }
                break;
                
            case HealthThreshold.Death:
                ChangeState(BossState.Defeat);
                break;
        }
    }

    void HandleIdleState()
    {
        // Wait for attack interval, then attack
        if (bossAttackComponent != null)
        {
            // 使用反射调用CanAttack方法
            System.Reflection.MethodInfo method = bossAttackComponent.GetType().GetMethod("CanAttack");
            if (method != null)
            {
                bool canAttack = (bool)method.Invoke(bossAttackComponent, null);
                if (canAttack)
                {
                    ChangeState(BossState.Attack);
                }
            }
        }
    }

    void HandleAttackState()
    {
        // Attack: fire laser, then return to idle
        if (bossAttackComponent != null)
        {
            // 使用反射调用PerformAttack方法
            System.Reflection.MethodInfo method = bossAttackComponent.GetType().GetMethod("PerformAttack");
            if (method != null)
            {
                method.Invoke(bossAttackComponent, null);
            }
        }
        ChangeState(BossState.Idle);
    }
    

    private bool isRunningAway = false;
    private Vector3 runAwayTarget;

    void HandleRunAwayState()
    {
        if (!isRunningAway)
        {
            // 计算玩家背后1.5米处的目标点
            Vector3 playerBack = cameraTransform.position - cameraTransform.forward * 1.5f;
            runAwayTarget = playerBack;

            // NavMesh采样，确保目标点可达且避开障碍物
            NavMeshHit hit;
            if (NavMesh.SamplePosition(runAwayTarget, out hit, 1.5f, NavMesh.AllAreas))
            {
                runAwayTarget = hit.position;
            }
            // 使用NavMeshAgent导航到目标点
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(runAwayTarget);
            }
            isRunningAway = true;
        }

        // 判断是否到达目标点或超时
        if ((navMeshAgent != null && !navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.2f) || stateTimer > 2f)
        {
            if (navMeshAgent != null)
            {
                navMeshAgent.ResetPath();
                navMeshAgent.isStopped = true;
            }
            isRunningAway = false;
            ChangeState(BossState.Idle);
        }
    }

    void HandleTakeDamageState()
    {
        // Play damage animation, then return to idle
        if (animator != null)
        {
            animator.Play("damage");
        }
        
        // Quickly return to idle
        if (stateTimer > 0.5f)
        {
            ChangeState(BossState.Idle);
        }
    }

    void HandleDefeatState()
    {
        if (!isDefeated)
        {
            isDefeated = true;
            if (animator != null)
            {
                animator.Play("defeat");
            }
            
            // Disable NavMeshAgent
            if (navMeshAgent != null)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.enabled = false;
            }
            
            Debug.Log("Boss Defeated!");
        }
    }

    public void ChangeState(BossState newState)
    {
        currentState = newState;
        stateTimer = 0f;
    }

    /// <summary>
    /// Forwards damage to the BossHealth component
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (currentState == BossState.Defeat) return;
        
        // Forward damage to health component
        if (bossHealth != null)
        {
            bossHealth.TakeDamage(amount);
        }
        else
        {
            Debug.LogError("BossHealth component not found!");
        }
    }

    // 攻击逻辑已移至BossAttack.cs
}
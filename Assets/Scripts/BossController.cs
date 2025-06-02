using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.MRUtilityKit;
using UnityEngine.AI;
using Unity.AI.Navigation;
using TMPro;

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
    private float timeSinceLastAttack = 0f;
    private bool isDefeated = false;
    private Transform cameraTransform;
    private Animator animator;
    private AudioSource backgroundMusic;
    private BossHealth bossHealth;
    
    // 用于控制日志输出频率的计数器
    private static int logCounter = 0;
    
    // 被攻击后增加逃跑权重的计时器和标志
    private float runAwayBoostTimer = 0f;
    private bool runAwayBoostActive = false;
    private float runAwayBoostDuration = 5.0f; // 逃跑权重提升持续5秒
    
    // 逃跑状态相关变量
    private bool isRunningAway = false;
    private Vector3 runAwayTarget = Vector3.zero;
    
    // Fuzzy logic decision system
    private Dictionary<BossState, float> stateWeights = new Dictionary<BossState, float>();
    [Header("Decision System")]
    public float attackWeight = 1.0f;         
    public float runAwayWeight = 1.5f;
    public float idleWeight = 1.0f;
    public float takeDamageWeight = 2.0f;
    public float defeatWeight = 10.0f;
    public float attackTimeFactor = 0.15f;    // 降低时间因子对攻击频率的影响
    public float healthFactor = 2.0f;         // 健康因子保持不变

    // NavMeshAgent
    [SerializeField] private TextMeshProUGUI debugTextComponent;
    [SerializeField] private TextMeshProUGUI debugAttackProbComponent;
    [SerializeField] private TextMeshProUGUI debugRunAwayProbComponent;
    [SerializeField] private TextMeshProUGUI debugIdleProbComponent;
    
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
        // Update state timer and attack timer
        stateTimer += Time.deltaTime;
        timeSinceLastAttack += Time.deltaTime;
        
        // 更新逃跑权重提升计时器
        if (runAwayBoostActive)
        {
            runAwayBoostTimer += Time.deltaTime;
            if (runAwayBoostTimer >= runAwayBoostDuration)
            {
                runAwayBoostActive = false;
                Debug.Log("Run away boost expired");
            }
        }
        
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
        
        // Always look at player
        if (currentState != BossState.Defeat && cameraTransform != null)
        {
            Vector3 targetDirection = cameraTransform.position - transform.position;
            targetDirection.y = 0; // Keep on same Y plane
            
            if (targetDirection != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 5f);
            }
        }
        
        // 评估下一个动作的频率
        // 每0.5秒评估一次，无论当前状态如何
        if (stateTimer > 1.0f) // 每0.5秒评估一次
        {
            EvaluateNextAction();
            stateTimer = 0f; // 重置状态计时器
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
                    
                    // 被攻击后激活逃跑权重提升
                    runAwayBoostActive = true;
                    runAwayBoostTimer = 0f;
                    Debug.Log("Boss was hit, increasing run away chance for the next few seconds");
                }
                break;
                
            // RunAway case removed - now handled by fuzzy logic system
                
            case HealthThreshold.Death:
                ChangeState(BossState.Defeat);
                break;
        }
    }

    void HandleIdleState()
    {
        // In the idle state, we just wait for the fuzzy logic system to decide the next action
        // The actual decision making happens in EvaluateNextAction()
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
                if (debugTextComponent != null)
                {
                    debugTextComponent.text = "Boss attacks";
                }
                method.Invoke(bossAttackComponent, null);
            }
        }
        // Reset the attack timer when attacking
        timeSinceLastAttack = 0f;
        ChangeState(BossState.Idle);
    }
    

    private void HandleRunAwayState()
    {
        if (!isRunningAway)
        {
            Debug.Log("Boss is starting to run away");
            if (debugTextComponent != null)
            {
                debugTextComponent.text = "Boss is starting to run away";
            }
            
            // 计算玩家背后1.5米处的目标点
            Vector3 playerBack = cameraTransform.position - cameraTransform.forward * 1.5f;
            runAwayTarget = playerBack;

            // NavMesh采样，确保目标点可达且避开障碍物
            NavMeshHit hit;
            bool foundValidPosition = false;
            int maxAttempts = 5; // 最多尝试5次
            float searchRadius = 1.5f; // 初始搜索半径
            
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                // 每次尝试增加搜索半径
                float currentRadius = searchRadius * (1 + attempt * 0.5f);
                
                // 在玩家背后的不同角度尝试寻找点位
                Vector3 directionOffset = Quaternion.Euler(0, Random.Range(-45f, 45f), 0) * -cameraTransform.forward;
                Vector3 attemptPosition = cameraTransform.position + directionOffset * (1.5f + attempt * 0.5f);
                
                if (NavMesh.SamplePosition(attemptPosition, out hit, currentRadius, NavMesh.AllAreas))
                {
                    runAwayTarget = hit.position;
                    Debug.Log($"Found valid run away target at {runAwayTarget} on attempt {attempt+1}");
                    foundValidPosition = true;
                    break;
                }
                
                Debug.Log($"Attempt {attempt+1} failed to find valid NavMesh position, trying again with radius {currentRadius}");
            }
            
            if (!foundValidPosition)
            {
                Debug.LogWarning("Failed to find valid NavMesh position after multiple attempts");
                // 如果实在找不到，就使用原始目标点
                runAwayTarget = cameraTransform.position - cameraTransform.forward * 2.0f;
            }
            
            // 使用NavMeshAgent导航到目标点
            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(runAwayTarget);
                Debug.Log("NavMeshAgent set destination for run away");
                // Add null check for debugTextUI before accessing its component
                if (debugTextComponent != null)
                {
                    debugTextComponent.text = "Boss runs away to destination: " + runAwayTarget;
                }
            }
            else
            {
                Debug.LogError("NavMeshAgent is null or disabled, cannot run away");
                if (debugTextComponent != null)
                {
                    debugTextComponent.text = "NavMeshAgent is null or disabled, cannot run away";
                }
            }
            
            isRunningAway = true;
        }

        // 判断是否到达目标点或超时
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            if ((!navMeshAgent.pathPending && navMeshAgent.remainingDistance < 0.2f) || stateTimer > 3.0f) // 增加逃跑时间
            {
                Debug.Log("Boss finished running away");
                navMeshAgent.ResetPath();
                navMeshAgent.isStopped = true;
                isRunningAway = false;
                ChangeState(BossState.Idle);
            }
        }
        else
        {
            // 如果没有NavMeshAgent，使用简单的计时器
            if (stateTimer > 3.0f)
            {
                Debug.Log("Boss run away timeout (no NavMeshAgent)");
                isRunningAway = false;
                ChangeState(BossState.Idle);
            }
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
        if (debugTextComponent != null)
        {
            debugTextComponent.text = "i am " + newState;
        }
        currentState = newState;
        stateTimer = 0f;
    }

    /// <summary>
    /// Evaluates which action to take next using a weight-based fuzzy logic system
    /// </summary>
    private void EvaluateNextAction()
    {
        // Skip decision making if defeated
        if (isDefeated) return;
        
        // Get current health percentage
        float healthPercentage = bossHealth != null ? bossHealth.GetHealthPercentage() : 1.0f;
        
        // Reset weights
        stateWeights.Clear();
        
        // Calculate weights for each state based on current conditions
        
        // Idle weight - higher when health is higher
        float idleWeightValue = idleWeight * healthPercentage;
        stateWeights[BossState.Idle] = idleWeightValue;
        
        // Attack weight - increases with time since last attack, decreases with lower health
        float attackTimeFactor = Mathf.Min(timeSinceLastAttack * this.attackTimeFactor, 2.0f); // Cap the time factor
        float attackWeightValue = attackWeight * attackTimeFactor * (0.5f + healthPercentage * 0.5f);
        stateWeights[BossState.Attack] = attackWeightValue;
        
        // Run away weight - now uses a random chance instead of being directly tied to health
        // This creates occasional run away behavior without requiring health threshold
        float runAwayChance = Random.Range(0f, 1f);
        float baseRunAwayWeight = runAwayWeight * (runAwayChance < 0.08f ? 1.5f : 0.1f); // 基础逃跑权重
        
        // 如果最近被攻击，大幅增加逃跑权重
        if (runAwayBoostActive)
        {
            // 权重提升随时间逐渐减弱
            float boostFactor = 1.0f - (runAwayBoostTimer / runAwayBoostDuration);
            float boostedWeight = baseRunAwayWeight + (3.0f * boostFactor); // 最多增加3.0的权重
            stateWeights[BossState.RunAway] = boostedWeight;
            
            if (logCounter % 10 == 0) // 使用与其他日志相同的频率控制
            {
                Debug.Log($"Run away weight boosted: {boostedWeight:F2} (base: {baseRunAwayWeight:F2}, boost: {3.0f * boostFactor:F2})");
            }
        }
        else
        {
            stateWeights[BossState.RunAway] = baseRunAwayWeight;
        }
        
        // TakeDamage weight - not directly chosen, triggered by damage events
        stateWeights[BossState.TakeDamage] = 0f;
        
        // Defeat weight - only relevant when health is zero
        stateWeights[BossState.Defeat] = healthPercentage <= 0 ? defeatWeight : 0f;
        
        // 只在较低频率下记录权重（每10次评估记录一次）
        // 使用静态计数器来控制日志频率
        if (++logCounter % 10 == 0) // 每10次评估才输出一次日志
        {
            string weightsLog = "State weights: ";
            foreach (var pair in stateWeights)
            {
                weightsLog += $"{pair.Key}: {pair.Value:F2}, ";
            }
            Debug.Log(weightsLog);
            logCounter = 0; // 重置计数器
        }
        
        // Choose the next state using weighted random selection
        BossState nextState = WeightedRandomSelection();
        
        // 不要转换到相同状态或TakeDamage状态（应由事件触发）
        if (nextState != currentState && nextState != BossState.TakeDamage)
        {
            // 只记录状态变化，不记录评估过程
            Debug.Log($"Boss changing state from {currentState} to {nextState} based on fuzzy logic decision");
            ChangeState(nextState);
            
            // 如果切换到攻击状态，重置攻击计时器
            if (nextState == BossState.Attack)
            {
                timeSinceLastAttack = 0f;
            }
        }
    }
    
    /// <summary>
    /// Selects a state using weighted random selection
    /// </summary>
    private BossState WeightedRandomSelection()
    {
        // Calculate total weight
        float totalWeight = 0f;
        foreach (var weight in stateWeights.Values)
        {
            totalWeight += weight;
        }

        // Debug log for state weights
        // Add null checks and format to 2 decimal places
        if (debugAttackProbComponent != null)
        {
            debugAttackProbComponent.text = (stateWeights[BossState.Attack]/totalWeight*100).ToString("F2") + "%";
        }
        if (debugRunAwayProbComponent != null)
        {
            debugRunAwayProbComponent.text = (stateWeights[BossState.RunAway]/totalWeight*100).ToString("F2") + "%";
        }
        if (debugIdleProbComponent != null)
        {
            debugIdleProbComponent.text = (stateWeights[BossState.Idle]/totalWeight*100).ToString("F2") + "%";
        }
        
        // If total weight is zero, default to idle
        if (totalWeight <= 0f)
        {
            return BossState.Idle;
        }
        
        // Random value between 0 and total weight
        float randomValue = Random.Range(0f, totalWeight);
        float cumulativeWeight = 0f;
        
        // Find the state corresponding to the random value
        foreach (var pair in stateWeights)
        {
            cumulativeWeight += pair.Value;
            if (randomValue <= cumulativeWeight)
            {
                return pair.Key;
            }
        }
        
        // Default to idle if something goes wrong
        return BossState.Idle;
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
}
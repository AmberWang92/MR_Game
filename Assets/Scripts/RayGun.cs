using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;

public class RayGun : MonoBehaviour
{
    [Header("Shooting button Setting")]
    public OVRInput.RawButton rightHandShootButton = OVRInput.RawButton.RIndexTrigger;
    public OVRInput.RawButton leftHandShootButton = OVRInput.RawButton.LIndexTrigger;
    
    [Header("Shooting Setting")]
    public float damage = 10f;         
    public float maxDistance = 5f;     
    public Transform muzzleTransform;  
    
    [Header("Visual Effect")]
    public GameObject laserPrefab;     
    public GameObject hitEffectPrefab;
    public float laserDuration = 0.3f; 
    
    [Header("Audio Effect")]
    public AudioClip fireSound;        
    public AudioClip pickupSound;
    
    [Header("VR Interaction")]
    public bool requireGrip = true;
    public bool hideControllerOnGrab = true; 
    
    [Header("Gun Position Adjustment")]
    public Vector3 handPositionOffset = new Vector3(0, 0, 0); 
    public Vector3 handRotationOffset = new Vector3(0, 90, 0); 
    public float vibrationDuration = 0.1f; 
    
    private AudioSource audioSource;
    private Rigidbody rb;
    private bool isGrabbed = false;
    private OVRInput.Controller activeController = OVRInput.Controller.None;
    private GameObject activeLaser; 
    
    // variables for detecting if grabbed 
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float movementThreshold = 0.01f;
    private float rotationThreshold = 0.5f;
    private float noMovementTimer = 0f;
    private float noMovementThreshold = 0.5f; 
    
    // controller related references
    private GameObject rightControllerVisual;
    private GameObject leftControllerVisual;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; 
        }
        
        // if muzzleTransform is not set, use the gun's position as default
        if (muzzleTransform == null)
        {
            // try to find the child object named "Muzzle"
            Transform muzzleChild = transform.Find("Muzzle");
            if (muzzleChild != null)
            {
                muzzleTransform = muzzleChild;
                Debug.Log("found the muzzle");
            }
            else
            {
                // if not found, use the gun's position
                muzzleTransform = transform;
            }
        }
        
        // record the initial position and rotation, used to detect movement
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        
        // check if the laser prefab is set
        if (laserPrefab == null)
        {
            Debug.LogWarning("laser prefab is not set");
        }
        
        // output muzzle position information
        Debug.Log($"muzzle position: {muzzleTransform.position}, muzzle forward: {muzzleTransform.forward}");
        
        // find controller visuals
        FindControllerVisuals();
    }
    
    // find controller visuals
    void FindControllerVisuals()
    {
        // find controller visuals based on Hierarchy structure
        GameObject controllerInteractions = GameObject.Find("[BuildingBlock] Controller Interactions");
        if (controllerInteractions != null)
        {
            // find left controller visuals
            Transform leftController = controllerInteractions.transform.Find("LeftController");
            if (leftController != null)
            {
                Transform visual = leftController.Find("OVRControllerVisual");
                if (visual != null)
                {
                    leftControllerVisual = visual.gameObject;
                    Debug.Log("left controller visuals found");
                }
            }
            
            // find right controller visuals
            Transform rightController = controllerInteractions.transform.Find("RightController");
            if (rightController != null)
            {
                Transform visual = rightController.Find("OVRControllerVisual");
                if (visual != null)
                {
                    rightControllerVisual = visual.gameObject;
                    Debug.Log("right controller visuals found");
                }
            }
        }
        else
        {
            Debug.LogWarning("[BuildingBlock] Controller Interactions node not found");
        }
        
        Debug.Log($"controllers visuals found - right: {(rightControllerVisual != null ? "found" : "not found")}, left: {(leftControllerVisual != null ? "found" : "not found")}");
    }
    
    void Update()
    {
        CheckIfGrabbed();
        
        // only check shooting input and update position when grabbed
        if (isGrabbed)
        {
            UpdateGunTransform();
            
            OVRInput.RawButton currentShootButton = 
                (activeController == OVRInput.Controller.RTouch) ? rightHandShootButton : leftHandShootButton;
            
            // if need to grip the trigger, check grip button
            bool gripPressed = false;
            if (activeController == OVRInput.Controller.RTouch)
            {
                gripPressed = OVRInput.Get(OVRInput.RawButton.RHandTrigger);
            }
            else if (activeController == OVRInput.Controller.LTouch)
            {
                gripPressed = OVRInput.Get(OVRInput.RawButton.LHandTrigger);
            }
            
            bool canShoot = !requireGrip || gripPressed;
            
            // check if shoot button is pressed
            if (canShoot && OVRInput.GetDown(currentShootButton))
            {
                Fire();
            }
        }
    }
    
    // update gun position and rotation to align with controller
    void UpdateGunTransform()
    {
        if (activeController == OVRInput.Controller.None)
            return;
            
        Transform controllerTransform = null;
        
        // 获取当前控制器的Transform
        if (activeController == OVRInput.Controller.RTouch)
        {
            GameObject rightHand = GameObject.Find("RightHandAnchor");
            if (rightHand != null)
                controllerTransform = rightHand.transform;
        }
        else if (activeController == OVRInput.Controller.LTouch)
        {
            GameObject leftHand = GameObject.Find("LeftHandAnchor");
            if (leftHand != null)
                controllerTransform = leftHand.transform;
        }
        
        if (controllerTransform != null)
        {
            Vector3 positionOffset = handPositionOffset;
            Quaternion rotationOffset = Quaternion.Euler(handRotationOffset);
            
            // 将偏移量从本地空间转换到世界空间 
            Vector3 worldPositionOffset = controllerTransform.TransformDirection(positionOffset);
            
            // 应用位置和旋转
            transform.position = controllerTransform.position + worldPositionOffset;
            transform.rotation = controllerTransform.rotation * rotationOffset;
        }
    }
    
    void CheckIfGrabbed()
    {
        // 检测移动和旋转 - 如果物体在移动或旋转，可能是被抓取了
        float movement = Vector3.Distance(transform.position, lastPosition);
        float rotation = Quaternion.Angle(transform.rotation, lastRotation);
        
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        
        if (movement > movementThreshold || rotation > rotationThreshold)
        {
            // 如果有明显移动或旋转，可能是被抓取了
            if (!isGrabbed)
            {
                // 尝试确定是哪只手抓取了射线枪
                DetermineGrabbingHand();
                OnGrabbed();
            }
            noMovementTimer = 0f;
        }
        else
        {
            // 如果没有移动，增加计时器
            noMovementTimer += Time.deltaTime;
            
            // 如果长时间没有移动，认为被放下了
            if (isGrabbed && noMovementTimer > noMovementThreshold)
            {
                OnReleased();
            }
        }
    }
    
    void DetermineGrabbingHand()
    {
        // 获取左右手控制器的位置
        Transform rightHand = GameObject.Find("RightHandAnchor")?.transform;
        Transform leftHand = GameObject.Find("LeftHandAnchor")?.transform;
        
        if (rightHand != null && leftHand != null)
        {
            // 计算距离
            float distToRight = Vector3.Distance(transform.position, rightHand.position);
            float distToLeft = Vector3.Distance(transform.position, leftHand.position);
            
            // 选择最近的控制器
            if (distToRight <= distToLeft)
            {
                activeController = OVRInput.Controller.RTouch;
            }
            else
            {
                activeController = OVRInput.Controller.LTouch;
            }
        }
        else if (rightHand != null)
        {
            activeController = OVRInput.Controller.RTouch;
        }
        else if (leftHand != null)
        {
            activeController = OVRInput.Controller.LTouch;
        }
    }
    
    void OnGrabbed()
    {
        isGrabbed = true;
        
        // 播放拾取音效
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
        
        // 设置物理属性
        if (rb != null)
        {
            rb.isKinematic = true; // 被抓取时不受物理影响
        }
        
        // 立即更新枪的位置和旋转
        UpdateGunTransform();
        
        // 隐藏对应的控制器模型
        if (hideControllerOnGrab)
        {
            SetControllerVisibility(false);
            Debug.Log("尝试隐藏控制器视觉模型");
        }
        
        Debug.Log($"射线枪被{(activeController == OVRInput.Controller.RTouch ? "右手" : "左手")}抓取");
    }
    
    void OnReleased()
    {
        isGrabbed = false;
        
        // 恢复物理属性
        if (rb != null)
        {
            rb.isKinematic = false;
            
            // 可选：给予一个初始速度，模拟扔出
            if (activeController != OVRInput.Controller.None)
            {
                Vector3 throwVelocity = OVRInput.GetLocalControllerVelocity(activeController);
                rb.linearVelocity = throwVelocity;
                
                Vector3 throwAngularVelocity = OVRInput.GetLocalControllerAngularVelocity(activeController);
                rb.angularVelocity = throwAngularVelocity;
            }
        }
        
        // 显示控制器模型
        if (hideControllerOnGrab)
        {
            SetControllerVisibility(true);
        }
        
        // 重置控制器
        activeController = OVRInput.Controller.None;
        
        Debug.Log("射线枪被释放");
    }
    
    // 设置控制器可见性
    void SetControllerVisibility(bool visible)
    {
        Debug.Log($"设置控制器可见性: {visible}, 活跃控制器: {activeController}");
        
        if (activeController == OVRInput.Controller.RTouch && rightControllerVisual != null)
        {
            rightControllerVisual.SetActive(visible);
            Debug.Log($"右手控制器已{(visible ? "显示" : "隐藏")}");
        }
        else if (activeController == OVRInput.Controller.LTouch && leftControllerVisual != null)
        {
            leftControllerVisual.SetActive(visible);
            Debug.Log($"左手控制器已{(visible ? "显示" : "隐藏")}");
        }
        else
        {
            Debug.LogWarning($"无法设置控制器可见性，控制器视觉模型可能未找到。右手: {rightControllerVisual != null}, 左手: {leftControllerVisual != null}");
        }
    }
    
    public void Fire()
    {
        Debug.Log("射线枪开火！");
        
        // 播放射击音效
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        // 确保枪口位置正确
        if (muzzleTransform == null)
        {
            Debug.LogError("枪口位置未设置！");
            muzzleTransform = transform;
        }
        
        // 射线起点和方向
        Vector3 rayOrigin = muzzleTransform.position;
        Vector3 rayDirection = muzzleTransform.forward;
        
        // 默认的激光线终点
        Vector3 endPoint = rayOrigin + rayDirection * maxDistance;
        
        // 射线检测
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxDistance))
        {
            // 更新激光线终点到击中点
            endPoint = hit.point;
            
            // 输出详细的调试信息
            Debug.Log($"射线击中: {hit.collider.name}, 位置: {hit.point}, 距离: {hit.distance}");
            
            // 检查是否击中了Ghost物体上的碰撞器
            bool hitGhostCollider = IsGhostCollider(hit.collider);
            
            // 如果击中Ghost的碰撞器，造成伤害
            if (hitGhostCollider)
            {
                // 查找Boss的BossHealth组件
                BossHealth bossHealth = GetBossHealthFromGhost(hit.transform);
                
                if (bossHealth != null)
                {
                    bossHealth.TakeDamage((int)damage);
                    Debug.Log($"击中Boss弱点！造成 {damage} 点伤害");
                }
                else
                {
                    Debug.LogWarning("击中了Ghost碰撞器，但未找到BossHealth组件");
                }
            }
            else
            {
                Debug.Log("射线未击中Boss弱点");
            }
            
            // 创建击中效果
            CreateHitEffect(hit.point);
        }
        else
        {
            Debug.Log($"射线未击中任何物体，终点: {endPoint}");
        }
        
        // 创建激光线
        CreateLaser(rayOrigin, endPoint);
        
        // 添加控制器震动反馈 - 使用可配置的震动持续时间
        if (activeController == OVRInput.Controller.RTouch)
        {
            OVRInput.SetControllerVibration(0.5f, 0.5f, OVRInput.Controller.RTouch);
            StartCoroutine(StopVibration(OVRInput.Controller.RTouch));
        }
        else if (activeController == OVRInput.Controller.LTouch)
        {
            OVRInput.SetControllerVibration(0.5f, 0.5f, OVRInput.Controller.LTouch);
            StartCoroutine(StopVibration(OVRInput.Controller.LTouch));
        }
    }
    
    // 检查是否是Ghost物体上的碰撞器
    private bool IsGhostCollider(Collider collider)
    {
        // 获取碰撞器所在的GameObject
        GameObject hitObject = collider.gameObject;
        
        // 检查该物体是否直接附加在名为"Ghost"的物体上
        if (hitObject.transform.parent != null && hitObject.transform.parent.name == "Ghost")
        {
            Debug.Log("击中了Ghost物体上的碰撞器");
            return true;
        }
        
        // 或者检查该物体本身是否名为"Ghost"
        if (hitObject.name == "Ghost")
        {
            Debug.Log("击中了Ghost物体本身");
            return true;
        }
        
        Debug.Log($"击中的物体不是Ghost的碰撞器: {hitObject.name}");
        return false;
    }
    
    // 从Ghost物体获取BossHealth组件
    private BossHealth GetBossHealthFromGhost(Transform hitTransform)
    {
        // 首先尝试找到Ghost物体
        Transform ghostTransform = hitTransform;
        
        // 如果击中的不是Ghost本身，尝试找到其父物体Ghost
        if (ghostTransform.name != "Ghost")
        {
            // 向上查找，直到找到名为"Ghost"的物体
            Transform current = ghostTransform.parent;
            while (current != null)
            {
                if (current.name == "Ghost")
                {
                    ghostTransform = current;
                    break;
                }
                current = current.parent;
            }
        }
        
        // 如果找到了Ghost物体
        if (ghostTransform.name == "Ghost")
        {
            // 根据您的预制体结构，BossHealth应该在Ghost的父物体上
            if (ghostTransform.parent != null)
            {
                BossHealth bossHealth = ghostTransform.parent.GetComponent<BossHealth>();
                if (bossHealth != null)
                {
                    Debug.Log($"在Ghost的父物体 {ghostTransform.parent.name} 上找到BossHealth组件");
                    return bossHealth;
                }
            }
            
            // 如果父物体上没有，尝试在根物体上查找
            BossHealth rootBossHealth = ghostTransform.root.GetComponent<BossHealth>();
            if (rootBossHealth != null)
            {
                Debug.Log($"在根物体 {ghostTransform.root.name} 上找到BossHealth组件");
                return rootBossHealth;
            }
        }
        
        Debug.LogWarning("未找到BossHealth组件");
        return null;
    }
    
    // 获取物体的完整路径，用于调试
    private string GetFullPath(Transform transform)
    {
        string path = transform.name;
        Transform parent = transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    // 检查是否击中了Ghost物体上的Capsule Collider
    private bool CheckIfHitGhostCapsule(Collider collider)
    {
        // 输出调试信息
        Debug.Log($"检查碰撞器: {collider.name}");
        
        // 检查是否是Capsule
        bool isCapsule = collider.name.ToLower().Contains("capsule");
        Debug.Log($"是否是Capsule: {isCapsule}");
        
        if (!isCapsule)
            return false;
            
        // 检查是否属于Ghost
        bool isPartOfGhost = false;
        Transform current = collider.transform;
        
        while (current != null)
        {
            Debug.Log($"检查父物体: {current.name}");
            
            if (current.name.ToLower().Contains("ghost"))
            {
                isPartOfGhost = true;
                Debug.Log($"找到Ghost父物体: {current.name}");
                break;
            }
            current = current.parent;
        }
        
        Debug.Log($"是否属于Ghost: {isPartOfGhost}");
        return isPartOfGhost;
    }
    
    // 查找BossHealth组件
    private BossHealth FindBossHealth(Transform hitTransform)
    {
        // 首先检查自身
        BossHealth bossHealth = hitTransform.GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            Debug.Log($"在击中物体上找到BossHealth组件");
            return bossHealth;
        }
        
        // 然后检查父物体
        Transform current = hitTransform.parent;
        while (current != null)
        {
            bossHealth = current.GetComponent<BossHealth>();
            if (bossHealth != null)
            {
                Debug.Log($"在父物体 {current.name} 上找到BossHealth组件");
                return bossHealth;
            }
            current = current.parent;
        }
        
        // 最后检查根物体
        bossHealth = hitTransform.root.GetComponent<BossHealth>();
        if (bossHealth != null)
        {
            Debug.Log($"在根物体 {hitTransform.root.name} 上找到BossHealth组件");
            return bossHealth;
        }
        
        Debug.LogWarning("未找到BossHealth组件");
        return null;
    }
    
    // 创建激光线
    void CreateLaser(Vector3 start, Vector3 end)
    {
        // 如果没有设置激光线预制体，直接返回
        if (laserPrefab == null)
        {
            Debug.LogError("激光线预制体未设置！");
            return;
        }
        
        // 如果已有激活的激光线，先销毁它
        if (activeLaser != null)
        {
            Destroy(activeLaser);
        }
        
        // 实例化激光线预制体
        activeLaser = Instantiate(laserPrefab, start, Quaternion.identity);
        Debug.Log($"创建激光线，起点: {start}, 终点: {end}");
        
        // 设置激光线的方向和长度
        Vector3 direction = end - start;
        float distance = direction.magnitude;
        
        // 如果激光线预制体包含LineRenderer组件
        LineRenderer lineRenderer = activeLaser.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
            Debug.Log("使用LineRenderer设置激光线位置");
        }
        else
        {
            // 如果没有LineRenderer，则调整预制体的缩放和旋转
            activeLaser.transform.up = direction.normalized;
            activeLaser.transform.localScale = new Vector3(
                activeLaser.transform.localScale.x,
                distance,
                activeLaser.transform.localScale.z
            );
            
            // 将预制体移动到起点和终点之间的中心
            activeLaser.transform.position = start + direction * 0.5f;
            Debug.Log("使用缩放和旋转设置激光线");
        }
        
        // 设置自动销毁
        Destroy(activeLaser, laserDuration);
    }
    
    // 创建击中效果
    void CreateHitEffect(Vector3 position)
    {
        // 如果没有设置击中效果预制体，直接返回
        if (hitEffectPrefab == null)
        {
            return;
        }
        
        // 实例化击中效果预制体
        GameObject hitEffect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
        Debug.Log($"创建击中效果，位置: {position}");
        
        // 设置自动销毁
        Destroy(hitEffect, 2.0f);
    }
    
    // 停止控制器震动的协程
    private IEnumerator StopVibration(OVRInput.Controller controller)
    {
        yield return new WaitForSeconds(vibrationDuration);
        OVRInput.SetControllerVibration(0, 0, controller);
    }
    
    // 当被Oculus Interaction系统抓取时调用
    public void NotifyGrabbed()
    {
        if (!isGrabbed)
        {
            OnGrabbed();
        }
    }
    
    // 当被Oculus Interaction系统释放时调用
    public void NotifyReleased()
    {
        if (isGrabbed)
        {
            OnReleased();
        }
    }
    
    // 在Unity编辑器中可视化枪口位置和方向
    void OnDrawGizmosSelected()
    {
        if (muzzleTransform != null)
        {
            // 绘制枪口位置
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(muzzleTransform.position, 0.02f);
            
            // 绘制射击方向
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(muzzleTransform.position, muzzleTransform.forward * 0.2f);
        }
    }
}
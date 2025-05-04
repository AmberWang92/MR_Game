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
    
    private AudioSource audioSource;
    private Rigidbody rb;
    private bool isGrabbed = false;
    private OVRInput.Controller activeController = OVRInput.Controller.None;
    private GameObject activeLaser; // 当前激活的激光线对象
    
    // 用于检测是否被抓取的变量
    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private float movementThreshold = 0.01f;
    private float rotationThreshold = 0.5f;
    private float noMovementTimer = 0f;
    private float noMovementThreshold = 0.5f; // 如果0.5秒没有移动，认为被放下
    
    void Start()
    {
        // 获取组件
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        
        // 确保有Rigidbody组件
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
        
        // 如果没有音频源，添加一个
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f; // 3D音效
        }
        
        // 如果没有设置枪口位置，使用自身位置
        if (muzzleTransform == null)
        {
            Debug.LogWarning("枪口位置(muzzleTransform)未设置，使用枪身位置作为默认值");
            
            // 尝试查找名为"Muzzle"的子物体
            Transform muzzleChild = transform.Find("Muzzle");
            if (muzzleChild != null)
            {
                muzzleTransform = muzzleChild;
                Debug.Log("自动找到名为'Muzzle'的子物体作为枪口");
            }
            else
            {
                // 如果找不到，使用枪身位置
                muzzleTransform = transform;
            }
        }
        
        // 记录初始位置和旋转，用于检测移动
        lastPosition = transform.position;
        lastRotation = transform.rotation;
        
        // 检查预制体是否已设置
        if (laserPrefab == null)
        {
            Debug.LogWarning("激光线预制体未设置，请在Inspector中设置laserPrefab");
        }
        
        // 输出枪口位置信息
        Debug.Log($"枪口位置: {muzzleTransform.position}, 枪口朝向: {muzzleTransform.forward}");
    }
    
    void Update()
    {
        // 检测是否被抓取
        CheckIfGrabbed();
        
        // 只有当被抓取时才检查射击输入
        if (isGrabbed)
        {
            // 根据当前使用的控制器选择正确的射击按钮
            OVRInput.RawButton currentShootButton = 
                (activeController == OVRInput.Controller.RTouch) ? rightHandShootButton : leftHandShootButton;
            
            // 如果需要握住扳机，检查握把按钮
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
            
            // 检测射击按钮
            if (canShoot && OVRInput.GetDown(currentShootButton))
            {
                Fire();
            }
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
        
        // 重置控制器
        activeController = OVRInput.Controller.None;
        
        Debug.Log("射线枪被释放");
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
        
        // 输出调试信息
        Debug.Log($"射击起点: {rayOrigin}, 方向: {rayDirection}");
        
        // 默认的激光线终点
        Vector3 endPoint = rayOrigin + rayDirection * maxDistance;
        
        // 射线检测
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxDistance))
        {
            // 更新激光线终点到击中点
            endPoint = hit.point;
            
            // 输出调试信息
            Debug.Log($"射线击中: {hit.collider.name}, 位置: {hit.point}, 距离: {hit.distance}");
            
            // 检查是否击中Boss
            BossHealth bossHealth = hit.collider.GetComponent<BossHealth>();
            if (bossHealth == null)
            {
                bossHealth = hit.collider.GetComponentInParent<BossHealth>();
            }
            
            // 如果击中Boss，造成伤害
            if (bossHealth != null)
            {
                bossHealth.TakeDamage((int)damage);
                Debug.Log($"击中Boss！造成 {damage} 点伤害");
            }
            else
            {
                Debug.Log($"射线击中物体: {hit.collider.name}，但未找到BossHealth组件");
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
        
        // 添加控制器震动反馈
        if (activeController == OVRInput.Controller.RTouch)
        {
            OVRInput.SetControllerVibration(0.5f, 0.5f, OVRInput.Controller.RTouch);
        }
        else if (activeController == OVRInput.Controller.LTouch)
        {
            OVRInput.SetControllerVibration(0.5f, 0.5f, OVRInput.Controller.LTouch);
        }
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction; // 添加Oculus交互命名空间

public class RayGun : MonoBehaviour
{
    [Header("射击按钮设置")]
    public OVRInput.RawButton rightHandShootButton = OVRInput.RawButton.RIndexTrigger; // 右手射击按钮
    public OVRInput.RawButton leftHandShootButton = OVRInput.RawButton.LIndexTrigger;  // 左手射击按钮
    
    [Header("射击设置")]
    public float damage = 10f;         // 伤害值
    public float maxDistance = 5f;    // 最大射程
    public Transform muzzleTransform;  // 枪口位置
    
    [Header("视觉效果")]
    public LineRenderer gunLine;     // 激光线效果
    public float laserDuration = 0.1f; // 激光线持续时间
    
    [Header("音效")]
    public AudioClip fireSound;        // 射击音效
    public AudioClip pickupSound;      // 拾取音效
    
    [Header("VR交互")]
    public bool requireGrip = true;    // 是否需要握住扳机才能射击
    
    private AudioSource audioSource;
    private Rigidbody rb;
    private bool isGrabbed = false;
    private OVRInput.Controller activeController = OVRInput.Controller.None; // 当前激活的控制器
    
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
            muzzleTransform = transform;
        }
        
        // 如果没有LineRenderer，添加一个
        if (gunLine == null)
        {
            gunLine = GetComponent<LineRenderer>();
            if (gunLine == null)
            {
                gunLine = gameObject.AddComponent<LineRenderer>();
                gunLine.startWidth = 0.05f;
                gunLine.endWidth = 0.05f;
                gunLine.material = new Material(Shader.Find("Sprites/Default"));
                gunLine.startColor = Color.red;
                gunLine.endColor = Color.red;
            }
        }
        
        // 确保激光线一开始是不可见的
        if (gunLine != null)
        {
            gunLine.enabled = false;
        }
        
        // 记录初始位置和旋转，用于检测移动
        lastPosition = transform.position;
        lastRotation = transform.rotation;
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
        // 播放射击音效
        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        // 射线起点和方向
        Vector3 rayOrigin = muzzleTransform.position;
        Vector3 rayDirection = muzzleTransform.forward;
        
        // 显示激光线
        StartCoroutine(ShowLaser(rayOrigin, rayOrigin + rayDirection * maxDistance));
        
        // 射线检测
        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, maxDistance))
        {
            // 显示激光线到击中点
            if (gunLine != null)
            {
                gunLine.SetPosition(1, hit.point);
            }
            
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
        }
    }
    
    IEnumerator ShowLaser(Vector3 start, Vector3 end)
    {
        if (gunLine != null)
        {
            gunLine.enabled = true;
            gunLine.SetPosition(0, start);
            gunLine.SetPosition(1, end);
            
            yield return new WaitForSeconds(laserDuration);
            
            gunLine.enabled = false;
        }
        else
        {
            yield return null;
        }
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
}
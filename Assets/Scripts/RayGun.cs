using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayGun : MonoBehaviour
{
    [Header("射击设置")]
    public float damage = 10f;         // 伤害值
    public float maxDistance = 20f;    // 最大射程
    public Transform muzzleTransform;  // 枪口位置
    
    [Header("视觉效果")]
    public LineRenderer laserLine;     // 激光线效果
    public float laserDuration = 0.1f; // 激光线持续时间
    
    [Header("音效")]
    public AudioClip fireSound;        // 射击音效
    
    private AudioSource audioSource;
    private Rigidbody rb;
    
    void Start()
    {
        // 获取组件
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        
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
        if (laserLine == null)
        {
            laserLine = GetComponent<LineRenderer>();
            if (laserLine == null)
            {
                laserLine = gameObject.AddComponent<LineRenderer>();
                laserLine.startWidth = 0.05f;
                laserLine.endWidth = 0.05f;
                laserLine.material = new Material(Shader.Find("Sprites/Default"));
                laserLine.startColor = Color.red;
                laserLine.endColor = Color.red;
            }
        }
        
        // 确保激光线一开始是不可见的
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }
    }
    
    void Update()
    {
        // 检测射击输入 - 在VR中，这里应该检测控制器按钮
        // 简单起见，这里使用鼠标点击
        if (Input.GetMouseButtonDown(0))
        {
            Fire();
        }
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
            if (laserLine != null)
            {
                laserLine.SetPosition(1, hit.point);
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
        if (laserLine != null)
        {
            laserLine.enabled = true;
            laserLine.SetPosition(0, start);
            laserLine.SetPosition(1, end);
            
            yield return new WaitForSeconds(laserDuration);
            
            laserLine.enabled = false;
        }
        else
        {
            yield return null;
        }
    }
}
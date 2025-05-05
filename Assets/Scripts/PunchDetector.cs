using UnityEngine;
using System.Collections;

public class PunchDetector : MonoBehaviour
{
    public float punchThreshold = 1.0f; 
    public float punchCooldown = 0.5f; 
    public int punchDamage = 5;
    
    [Header("Haptic Feedback")]
    public OVRInput.Controller controllerType = OVRInput.Controller.None; // 设置为LTouch或RTouch

    private Vector3 lastPosition;
    private float lastPunchTime = 0f;
    private HapticTrigger hapticTrigger;

    void Start()
    {
        lastPosition = transform.position;
        
        // 获取HapticTrigger组件（假设已经手动添加）
        hapticTrigger = GetComponent<HapticTrigger>();
        if (hapticTrigger == null)
        {
            Debug.LogWarning("未找到HapticTrigger组件，请手动添加该组件到" + gameObject.name);
        }
    }

    void Update()
    {
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;

        if (velocity.magnitude > punchThreshold && Time.time > lastPunchTime + punchCooldown)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, velocity.normalized, out hit, 0.3f))
            {
                if (hit.collider.CompareTag("Boss"))
                {
                    hit.collider.GetComponentInParent<BossHealth>().TakeDamage(punchDamage);
                    Debug.Log("Boss Hit!");
                    lastPunchTime = Time.time;
                    
                    // 触发控制器震动
                    if (hapticTrigger != null && controllerType != OVRInput.Controller.None)
                    {
                        hapticTrigger.TriggerHaptics(controllerType);
                        Debug.Log("触发" + controllerType.ToString() + "控制器震动");
                    }
                }
            }
        }

        lastPosition = transform.position;
    }

// 绘制射线
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
            if (velocity.magnitude > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, velocity.normalized * 0.3f);
            }
        }
    }
}

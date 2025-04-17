using UnityEngine;

public class PunchDetector : MonoBehaviour
{
    public float punchThreshold = 1.0f; // 调整为合适的速度阈值
    public float punchCooldown = 0.5f; // 防止短时间内重复攻击
    public int punchDamage = 10;

    private Vector3 lastPosition;
    private float lastPunchTime = 0f;

    void Start()
    {
        lastPosition = transform.position;
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
                }
            }
        }

        lastPosition = transform.position;
    }

    //可视化射线，方便调试拳击方向
    void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, velocity.normalized * 0.3f);
        }
    }
}

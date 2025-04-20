using UnityEngine;


public class PunchDetector : MonoBehaviour
{
    public float punchThreshold = 1.0f; // ����Ϊ���ʵ��ٶ���ֵ
    public float punchCooldown = 0.5f; // ��ֹ��ʱ�����ظ�����
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
                    //MRHaptics.PlayHapticImpulse(hand, 0.7f, 0.25f);
                }
            }
        }

        lastPosition = transform.position;
    }

    //���ӻ����ߣ��������ȭ������
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

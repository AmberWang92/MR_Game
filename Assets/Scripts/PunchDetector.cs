using UnityEngine;
using System.Collections;

public class PunchDetector : MonoBehaviour
{
    //this scripts attach to the 2 controllers of the player
    public float punchThreshold = 1.0f; 
    public float punchCooldown = 0.5f; 
    public int punchDamage = 5;
    
    [Header("Haptic Feedback")]
    public OVRInput.Controller controllerType = OVRInput.Controller.None; 

    private Vector3 lastPosition;
    private float lastPunchTime = 0f;
    private HapticTrigger hapticTrigger;

    void Start()
    {
        lastPosition = transform.position;
        
        hapticTrigger = GetComponent<HapticTrigger>();
        if (hapticTrigger == null)
        {
            Debug.LogWarning("HapticTrigger component not found on " + gameObject.name);
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
                    
                    lastPunchTime = Time.time;
                    
                    if (hapticTrigger != null && controllerType != OVRInput.Controller.None)
                    {
                        hapticTrigger.TriggerHaptics(controllerType);
                    }
                }
            }
        }

        lastPosition = transform.position;
    }

// draw ray
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

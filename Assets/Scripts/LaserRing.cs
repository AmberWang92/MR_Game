using UnityEngine;

public class LaserRing : MonoBehaviour
{
    public float duration = 5f;         // 激光持续时间
    public float expandSpeed = 0.8f;    // 每秒扩大多少倍

    private float timer = 0f;
    private Vector3 initialScale;
   
    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 缓慢扩大
        float scaleFactor = 1f + expandSpeed * timer;
        transform.localScale = initialScale * scaleFactor;

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}

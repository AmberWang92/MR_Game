using UnityEngine;

public class LaserRing : MonoBehaviour
{
    public float duration = 5f;         // �������ʱ��
    public float expandSpeed = 0.8f;    // ÿ��������ٱ�

    private float timer = 0f;
    private Vector3 initialScale;
   
    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        timer += Time.deltaTime;

        // ��������
        float scaleFactor = 1f + expandSpeed * timer;
        transform.localScale = initialScale * scaleFactor;

        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }
}

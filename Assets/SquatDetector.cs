using UnityEngine;
using UnityEngine.Events;

public class SquatDetector : MonoBehaviour
{
    public float squatThresholdY = 1.2f; // ͷ���߶�С�ڴ�ֵ����Ϊ����
    public UnityEvent onSquat;
    public UnityEvent onStand;

    private bool isSquatting = false;

    void Update()
    {
        float headY = transform.position.y;

        if (!isSquatting && headY < squatThresholdY)
        {
            isSquatting = true;
            onSquat?.Invoke();
            Debug.Log("Player Squatted");
        }
        else if (isSquatting && headY >= squatThresholdY)
        {
            isSquatting = false;
            onStand?.Invoke();
            Debug.Log("Player Stood Up");
        }
    }
}

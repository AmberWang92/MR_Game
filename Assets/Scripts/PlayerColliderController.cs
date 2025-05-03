using UnityEngine;

public class PlayerColliderController: MonoBehaviour
{
    public CapsuleCollider capsuleCollider;
    public Transform headsetTransform; // 头戴设备的Transform，通常是Camera或CenterEyeAnchor
    public float minHeight = 0.5f;     // 蹲下时的最小高度
    public float maxHeight = 1.7f;     // 站立时的最大高度
    
    [Header("Squatting Detection")]
    public float standingHeight = 1.6f;  // 玩家正常站立时的大致高度
    public float squatThreshold = 0.7f;  // 高度低于正常高度的这个比例时视为蹲下
    
    [HideInInspector]
    public bool isSquatting = false;     // 当前是否处于蹲下状态
    
    private float initialHeadHeight;     // 初始头部高度，用于校准
    private bool isCalibrated = false;   // 是否已校准

    void Reset()
    {
        // 自动获取同GameObject上的CapsuleCollider
        if (!capsuleCollider) capsuleCollider = GetComponent<CapsuleCollider>();
    }
    
    void Start()
    {
        // 在开始时进行校准
        CalibrateHeight();
    }

    void Update()
    {
        if (!capsuleCollider || !headsetTransform) return;
        
        // 如果还没校准，尝试校准
        if (!isCalibrated && headsetTransform.position.y > 0.5f)
        {
            CalibrateHeight();
        }
        
        // 获取CameraRig（假设是capsuleCollider的父物体）
        Transform cameraRig = capsuleCollider.transform.parent;
        
        // 将头戴设备的世界坐标转换为CameraRig的局部坐标
        Vector3 localHeadPosition = cameraRig.InverseTransformPoint(headsetTransform.position);
        
        // 计算当前头部高度相对于地面的高度
        float currentHeadHeight = localHeadPosition.y;
        
        // 计算局部坐标系中的碰撞体高度
        float colliderHeight = Mathf.Clamp(
            currentHeadHeight - capsuleCollider.transform.localPosition.y,
            minHeight,
            maxHeight
        );
        
        // 更新碰撞体高度
        capsuleCollider.height = colliderHeight;
        
        // 调整碰撞体的位置，使其跟随玩家下蹲
        // 计算碰撞体应该在的Y位置(从头部位置减去半个碰撞体高度)
        float newY = localHeadPosition.y - (colliderHeight / 2);
        
        // 更新碰撞体位置
        Vector3 newPosition = capsuleCollider.transform.localPosition;
        newPosition.y = newY;
        capsuleCollider.transform.localPosition = newPosition;
        
        // 碰撞体中心保持在本地坐标原点
        capsuleCollider.center = Vector3.zero;
        
        // 检测玩家是否蹲下 - 使用相对于校准高度的比例
        if (isCalibrated)
        {
            // 如果当前高度低于阈值，判定为蹲下
            bool newSquatState = currentHeadHeight < (initialHeadHeight * squatThreshold);
            
            // 状态变化时输出日志
            if (newSquatState != isSquatting)
            {
                isSquatting = newSquatState;
                Debug.Log("Player " + (isSquatting ? "Squatted" : "Stood Up") + 
                          " - Height: " + currentHeadHeight.ToString("F2") + 
                          " (Threshold: " + (initialHeadHeight * squatThreshold).ToString("F2") + ")");
            }
        }
    }
    
    // 校准玩家高度
    private void CalibrateHeight()
    {
        if (!headsetTransform) return;
        
        Transform cameraRig = capsuleCollider.transform.parent;
        Vector3 localHeadPosition = cameraRig.InverseTransformPoint(headsetTransform.position);
        
        // 记录初始头部高度
        initialHeadHeight = localHeadPosition.y;
        isCalibrated = true;
        
        Debug.Log("Player height calibrated: " + initialHeadHeight.ToString("F2") + "m");
    }
    
    // 公共方法，供其他脚本检查玩家是否蹲下
    public bool IsPlayerSquatting()
    {
        return isSquatting;
    }
}

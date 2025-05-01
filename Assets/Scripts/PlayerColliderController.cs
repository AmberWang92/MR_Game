using UnityEngine;

public class PlayerColliderController: MonoBehaviour
{
    public CapsuleCollider capsuleCollider;
    public Transform headsetTransform; // ͷ�Ե� Transform��ͨ���� Camera �� CenterEyeAnchor��
    public float minHeight = 0.5f;     // ����ʱ����С�߶�
    public float maxHeight = 1.7f;     // վ��ʱ�����߶�

    void Reset()
    {
        // �Զ���ȡͬ GameObject �ϵ� CapsuleCollider
        if (!capsuleCollider) capsuleCollider = GetComponent<CapsuleCollider>();
    }

   void Update()
{
    if (!capsuleCollider || !headsetTransform) return;
    
    // 获取 CameraRig（假设是 capsuleCollider 的父物体）
    Transform cameraRig = capsuleCollider.transform.parent;
    
    // 将头戴设备的世界坐标转换为 CameraRig 的局部坐标
    Vector3 localHeadPosition = cameraRig.InverseTransformPoint(headsetTransform.position);
    
    // 计算局部坐标系中的高度
    float headHeight = Mathf.Clamp(
        localHeadPosition.y - capsuleCollider.transform.localPosition.y,
        minHeight,
        maxHeight
    );
    
    // 更新碰撞体高度
    capsuleCollider.height = headHeight;
    
    // 关键改进：调整碰撞体的位置，使其跟随玩家下蹲
    // 计算碰撞体应该在的Y位置 (从头部位置减去半个碰撞体高度)
    float newY = localHeadPosition.y - (headHeight / 2);
    
    // 更新碰撞体位置和中心点
    Vector3 newPosition = capsuleCollider.transform.localPosition;
    newPosition.y = newY;
    capsuleCollider.transform.localPosition = newPosition;
    
    // 碰撞体中心保持在本地坐标原点
    capsuleCollider.center = Vector3.zero;
}
}

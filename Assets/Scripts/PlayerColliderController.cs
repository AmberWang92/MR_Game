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

         float headHeight = Mathf.Clamp(
            headsetTransform.position.y - capsuleCollider.transform.position.y,
            minHeight,
            maxHeight
        );
        capsuleCollider.height = headHeight;
        capsuleCollider.center = new Vector3(0, headHeight / 2f, 0);
    }
}
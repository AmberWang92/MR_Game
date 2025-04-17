using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class BossController : MonoBehaviour
{
    public ParticleSystem inkEffect;
    public float inkDuration = 3f;
    public float attackInterval = 8f;

    private bool isSpraying = false;
    private float timer = 0f;
    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;

        // ע�᳡��������ɵĻص�
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
    }

    void OnSceneLoaded()
    {
        Debug.Log("MRUK Initialized");

        // ��ȡ��ǰ����͵ذ� Anchor
        var currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom == null || currentRoom.FloorAnchor == null)
        {
            Debug.LogWarning("No room or floor anchor found!");
            return;
        }

        var floorAnchor = currentRoom.FloorAnchor;

        // ��ȡ����ӽǲ�����
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        // ����Ŀ��λ�ã�������� 1.5 ��Զ�ĵ����Ϸ� 1.5 ��
        Vector3 targetPos = cameraTransform.position + forward * 1.5f;
        targetPos.y = floorAnchor.transform.position.y + 0.3f; // �����ڵ����� 0.5m

        //// �ƶ� Boss ����λ��
        transform.position = targetPos;

        Debug.Log("Boss moved to position: " + targetPos);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // ʼ�ճ������
        Vector3 lookAt = new Vector3(cameraTransform.position.x, transform.position.y, cameraTransform.position.z);
        transform.LookAt(lookAt);

        if (timer >= attackInterval)
        {
            StartCoroutine(SprayInk());
            timer = 0f;
        }
    }

    IEnumerator SprayInk()
    {
        Debug.Log("Ink Attack Incoming!");
        isSpraying = true;

        inkEffect.Play();
        yield return new WaitForSeconds(inkDuration);
        inkEffect.Stop();

        isSpraying = false;
    }

    public bool IsSpraying()
    {
        return isSpraying;
    }
}
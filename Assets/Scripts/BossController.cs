using System.Collections;
using UnityEngine;
using Meta.XR.MRUtilityKit;

public class BossController : MonoBehaviour
{
    //public ParticleSystem inkEffect;
    //public float inkDuration = 3f;
    //private bool isSpraying = false;

    public float attackInterval = 8f;
    private float timer = 0f;
    private Transform cameraTransform;

    public GameObject laserRingPrefab;  
    public Transform laserSpawnPoint;   
    public float laserDuration = 5f;    

    void Start()
    {
        cameraTransform = Camera.main.transform;

        // 注册场景加载完成的回调
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
    }

    void OnSceneLoaded()
    {
        // 获取当前房间和地板 Anchor
        var currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom == null || currentRoom.FloorAnchor == null)
        {
            Debug.LogWarning("No room or floor anchor found!");
            return;
        }

        var floorAnchor = currentRoom.FloorAnchor;

        // 获取玩家视角并朝向
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        // 计算目标位置：距离玩家 1.5 米远的地面上方 1.5 米
        Vector3 targetPos = cameraTransform.position + forward * 1.5f;
        targetPos.y = floorAnchor.transform.position.y + 0.3f; // 悬浮在地面上 0.3m

        //// 移动 Boss 到该位置
        transform.position = targetPos;

        Debug.Log("Boss moved to position: " + targetPos);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // 始终朝向玩家
        Vector3 lookAt = new Vector3(cameraTransform.position.x, transform.position.y, cameraTransform.position.z);
        transform.LookAt(lookAt);

        if (timer >= attackInterval)
        {
            //StartCoroutine(SprayInk());
            StartCoroutine(FireLaserRing());
            timer = 0f;
        }
    }

    //IEnumerator SprayInk()
    //{
    //Debug.Log("Ink Attack Incoming!");
    //isSpraying = true;

    //    inkEffect.Play();
    //    yield return new WaitForSeconds(inkDuration);
    //    inkEffect.Stop();

    //    isSpraying = false;
    //}

    IEnumerator FireLaserRing()
    {
        Debug.Log("Laser Attack Incoming!");
        GameObject laser = Instantiate(laserRingPrefab, laserSpawnPoint.position, Quaternion.identity);


        laser.transform.rotation = Quaternion.LookRotation(Vector3.up);
        Debug.Log("Laser Spawn Position: " + laserSpawnPoint);

        yield return null;
    }

    //public bool IsSpraying()
    //{
    //    return isSpraying;
    //}
}
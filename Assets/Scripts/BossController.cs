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

    // 添加Animator引用
    private Animator animator;
    
    // 背景音乐管理器引用
    private AudioSource backgroundMusic;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        
        // 获取子物体上的Animator组件
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("No Animator component found in Boss or its children!");
        }

        // 添加背景音乐组件
        backgroundMusic = gameObject.AddComponent<AudioSource>();
        
        // 查找音乐资源
        AudioClip bossMusic = Resources.Load<AudioClip>("BossMusic");
        if (bossMusic != null)
        {
            backgroundMusic.clip = bossMusic;
            backgroundMusic.loop = true;
            backgroundMusic.volume = 0.1f;
            backgroundMusic.Play();
            Debug.Log("Boss Music started");
        }
        else
        {
            Debug.LogWarning("Boss Music not found");
        }

        // Register scene loaded callback
        // 当Unity场景加载完成时，自动调用BossController里的OnSceneLoaded方法
        MRUK.Instance.RegisterSceneLoadedCallback(OnSceneLoaded);
    }

    void OnSceneLoaded()
    {
        // Get current room and floor anchor
        var currentRoom = MRUK.Instance.GetCurrentRoom();
        if (currentRoom == null || currentRoom.FloorAnchor == null)
        {
            Debug.LogWarning("No room or floor anchor found!");
            return;
        }

        var floorAnchor = currentRoom.FloorAnchor;

        // Get player forward direction
        Vector3 forward = cameraTransform.forward;
        forward.y = 0;
        forward.Normalize();

        // Calculate target position: 1.5m away from player, 0.3m above floor
        Vector3 targetPos = cameraTransform.position + forward * 1.5f;
        targetPos.y = floorAnchor.transform.position.y + 0.3f;

        // Move Boss to that position
        transform.position = targetPos;

        Debug.Log("Boss moved to position: " + targetPos);
    }

    void Update()
    {
        timer += Time.deltaTime;

        // Always face the player
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
        // 触发攻击动画
        if (animator != null)
        {
            animator.Play("attack");
        }

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
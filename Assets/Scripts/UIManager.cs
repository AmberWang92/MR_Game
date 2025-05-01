using UnityEngine;
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    public GameObject deathUI;
    public Transform bossTransform;
    public Transform playerTransform;
    

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        HideDeathUI();

    }


    void Update()
    {
        // 只有 deathUI 激活时才同步
        if (deathUI != null && deathUI.activeSelf && bossTransform != null && playerTransform != null)
        {
            // Calculate a position 1 meter in front of the player and 1 meter higher
            Vector3 directionToPlayer = (playerTransform.position - bossTransform.position).normalized;
            deathUI.transform.position = playerTransform.position - directionToPlayer + new Vector3(0, 1f, 0);

            Debug.Log("Boss Position: " + bossTransform.position);

            // Ensure the UI faces the player
            deathUI.transform.LookAt(playerTransform);

            // Correct the rotation to maintain horizontal orientation and rotate 90 degrees outward
            deathUI.transform.Rotate(60, 180, 0);
        }
    }

    public void ShowDeathUI()
    {
        // 获取 Boss Transform
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");
        if (boss != null)
        {
            bossTransform = boss.transform;
        }

        if (deathUI != null)
            deathUI.SetActive(true);
    }

    public void HideDeathUI()
    {
        if (deathUI != null)
            deathUI.SetActive(false);
    }
}

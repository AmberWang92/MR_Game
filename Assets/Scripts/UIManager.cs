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
            // 跟随 boss 位置（可加偏移）
            deathUI.transform.position = bossTransform.position + new Vector3(0, 1f, -0.5f); // 2f为高度偏移，可调整

            Debug.Log("Boss Position: " + bossTransform.position);

            // 朝向 player
            deathUI.transform.LookAt(playerTransform);
    
            // 让 UI 正面朝向 player（如果 UI 反了，加180度旋转）
            deathUI.transform.Rotate(0, 180, 0);
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

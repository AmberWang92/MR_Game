using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public Image healthBarImage;

    private int currentHealth;
    private Animator animator;
    
    // 动画触发器参数名
    private const string HURT_TRIGGER = "Hurt";
    private static readonly int DissolveState = Animator.StringToHash("Base Layer.dissolve");

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBarImage) healthBarImage.fillAmount = maxHealth/100;
        
        // 获取子物体上的Animator组件
        animator = GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("No Animator component found in Boss or its children!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Die();
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Max(0, currentHealth);

        if (healthBarImage) healthBarImage.fillAmount = currentHealth/100.0f;

        // 触发受伤动画
        if (animator != null && currentHealth > 0)
        {
            animator.SetTrigger(HURT_TRIGGER);
            Debug.Log("Boss hurt animation triggered");
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Boss Defeated!");

        // 停止背景音乐
        AudioSource bossMusic = GetComponent<AudioSource>();
        if (bossMusic != null && bossMusic.isPlaying)
        {
            // 淡出音乐
            StartCoroutine(FadeOutMusic(bossMusic, 2.0f));
        }
        
        // 触发死亡动画
        if (animator != null)
        {
            animator.Play("surprised");
        }
        
        Invoke("DestroyAfterAnimation", 3f); // 假设动画持续3秒
    }
    
    // 音乐淡出协程
    private IEnumerator FadeOutMusic(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float startTime = Time.time;
        
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            audioSource.volume = Mathf.Lerp(startVolume, 0, t);
            yield return null;
        }
        
        audioSource.Stop();
        audioSource.volume = startVolume; // 恢复原始音量，以防万一
    }
    
    void DestroyAfterAnimation()
    {
        //TODO: 可以在这里添加游戏胜利的逻辑
        if(UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictoryUI();
        }
        Destroy(gameObject);
    }
}

using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;

    void Start()
    {
        currentLives = maxLives;
        UpdateHeartsUI();
    }

    public void TakeDamage(int amount = 1)
    {
        currentLives -= amount;
        currentLives = Mathf.Max(currentLives, 0);

        Debug.Log("Player hit! Lives left: " + currentLives);

        UpdateHeartsUI();

        if (currentLives <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player died!");
        // TODO: 玩家死亡逻辑，例如暂停游戏、重置场景、播放动画等
    }

    void UpdateHeartsUI()
    {
        // TODO: 这里之后可以挂心形 UI 更新逻辑
        Debug.Log("❤️ x " + currentLives);
    }

    public int GetLives()
    {
        return currentLives;
    }
}

using Oculus.Haptics;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int maxLives = 3;
    private int currentLives;

    public float restartDelay = 3f;

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
        GetComponent<HapticTrigger>().TriggerHaptics(OVRInput.Controller.RTouch);
        GetComponent<HapticTrigger>().TriggerHaptics(OVRInput.Controller.LTouch);

        UpdateHeartsUI();

        if (currentLives <= 0)
        {
            Die();
        }
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

    void Die()
    {
        Debug.Log("Player died!");
        UIManager.Instance.ShowDeathUI();
    }
   public void Restart()
{
    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
}
    public void QuitGame()
    {
        Application.Quit();
    }

}

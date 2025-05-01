using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [Header("Button")]
    public Button startButton;
    public Button quitButton;

    void Start()
    {
        // 设置按钮监听器
        if (startButton) startButton.onClick.AddListener(StartGame);
        if (quitButton) quitButton.onClick.AddListener(QuitGame);
    }
    
    public void StartGame()
    {
        // 检查是否应该使用SceneTransitionManager或直接使用SceneManager
        //if (FindObjectOfType<SceneTransitionManager>() != null)
        //{
          //  SceneTransitionManager.singleton.GoToSceneAsync(1);
        //}
        //else
        //{
            // 直接加载下一个场景
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
        //}
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
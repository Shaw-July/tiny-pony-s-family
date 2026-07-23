using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pausePanel;

    private bool isPaused;

    private void Start()
    {
        // 确保游戏开始时不是暂停状态
        Time.timeScale = 1f;
        isPaused = false;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void PauseGame()
    {
        if (isPaused)
        {
            return;
        }

        isPaused = true;
        pausePanel.SetActive(true);

        // 停止所有依赖 Time.deltaTime 的游戏逻辑
        Time.timeScale = 0f;
    }

    public void ContinueGame()
    {
        if (!isPaused)
        {
            return;
        }

        isPaused = false;
        pausePanel.SetActive(false);

        // 恢复游戏时间
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {
        // 重新加载场景之前必须恢复时间
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        // 防止进入其他场景后仍然保持暂停
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
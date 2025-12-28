using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject pausePanel;
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Button exitButton;
    public GameObject pauseOverlay;
    public PlayerCharacter player;

    private bool isPaused = false;

    void Start()
    {
        // Hide pause panel at start
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (pauseOverlay != null) 
        {
            pauseOverlay.SetActive(false);
        }

        // Setup button listeners
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }

        if (mainMenuButton != null)
        {
            // Main menu functionality to be implemented later
            mainMenuButton.onClick.AddListener(() => {
                // TODO: Load main menu scene
            });
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(ExitGame);
        }

        // Auto-find player if not assigned
        if (player == null)
        {
            player = FindObjectOfType<PlayerCharacter>();
        }
    }

    void Update()
    {
        // Check for ESC key to toggle pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        if (isPaused) return;

        isPaused = true;

        // Stop time
        Time.timeScale = 0f;

        // Pause timer
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.PauseTimer();
        }

        // Show pause panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // Disable player input
        if (player != null)
        {
            player.SetPaused(true);
        }

        if (pauseOverlay != null)
            pauseOverlay.SetActive(true);
    }

    public void ResumeGame()
    {
        if (!isPaused) return;

        isPaused = false;

        // Resume time
        Time.timeScale = 1f;

        // Resume timer (only if not selecting buff)
        if (GameTimer.Instance != null && (BuffManager.Instance == null || !BuffManager.Instance.IsSelectingBuff()))
        {
            GameTimer.Instance.ResumeTimer();
        }

        // Hide pause panel
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Enable player input
        if (player != null)
        {
            player.SetPaused(false);
        }

        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(false);
        }
    }

    public void RestartGame()
    {
        // Reset buffs before reloading scene
        if (BuffManager.Instance != null)
        {
            BuffManager.Instance.ResetBuffs();
        }

        // Reset timer
        if (GameTimer.Instance != null)
        {
            GameTimer.Instance.ResetTimer();
            GameTimer.Instance.StartTimer();
        }

        // Resume time before reloading (important!)
        Time.timeScale = 1f;

        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        // Resume time before exiting (important!)
        Time.timeScale = 1f;

        // Exit application
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}